using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Hangfire;
using PochinkiBot.Background.Jobs;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;
using Serilog;

namespace PochinkiBot.Client
{
    public class DiscordClient
    {
        private readonly BotConfig _config;
        private readonly IRedisDatabaseProvider _redisDatabaseProvider;
        private readonly IRouletteStore _rouletteStore;
        private readonly IPidorStore _pidorStore;
        private readonly IRemoveRoleJob _removeRoleJob;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger = Log.Logger;

        private const string RoulettePattern = @"^рулетка!$";
        private const string StatsPattern = @"^статистика!$";
        private const string WhoPidorPattern = @"^кто пидор\?$";
        private const string AmIPidorPattern = @"^я пидор\?$";
        private const string GuildPidorTopPattern = @"^перепись пидоров$";
        private const string DefaultPidorRole = "пидор дня";

        private bool _pidorSearchActive;

        private readonly Random _rng = new Random((int)DateTime.UtcNow.Ticks);

        public DiscordClient(BotConfig config,
            IRedisDatabaseProvider redisDatabaseProvider,
            IRouletteStore rouletteStore,
            IPidorStore pidorStore,
            IRemoveRoleJob removeRoleJob,
            DiscordSocketClient client,
            IBackgroundJobClient backgroundJobClient)
        {
            _config = config;
            _redisDatabaseProvider = redisDatabaseProvider;
            _rouletteStore = rouletteStore;
            _pidorStore = pidorStore;
            _removeRoleJob = removeRoleJob;
            _backgroundJobClient = backgroundJobClient;
            _client = client;
        }

        public async Task WaitForShutdown()
        {
            using (_client)
            {
                ConfigureEvents();

                await _client.LoginAsync(TokenType.Bot, _config.Token);
                await _client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private void ConfigureEvents()
        {
            _client.Log += msg =>
            {
                var logger = _logger.ForContext("Source", msg.Source);
                switch (msg.Severity)
                {
                    case LogSeverity.Critical:
                        logger.Fatal(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Error:
                        logger.Error(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Warning:
                        logger.Warning(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Info:
                        logger.Information(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Verbose:
                        logger.Verbose(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Debug:
                        logger.Debug(msg.Exception, msg.Message);
                        break;
                }
                return Task.CompletedTask;
            };
            _client.Ready += () => _redisDatabaseProvider.Database.StringSetAsync("StartedAt", DateTime.UtcNow.ToString("O"));
            _client.MessageReceived += OnClientOnMessageReceived;
        }

        private Task OnClientOnMessageReceived(SocketMessage msg)
        {
            Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage userMessage))
                    return;

                var pos = 0;
                if (!userMessage.HasMentionPrefix(_client.CurrentUser, ref pos))
                    return;

                var content = msg.Content.Substring(pos);

                if (Regex.IsMatch(content, RoulettePattern, RegexOptions.IgnoreCase))
                    await PlayRoulette(userMessage);
                else if (Regex.IsMatch(content, StatsPattern, RegexOptions.IgnoreCase))
                    await GetStats(userMessage);
                else if (Regex.IsMatch(content, WhoPidorPattern, RegexOptions.IgnoreCase))
                    await WhoIsPidor(userMessage);
                else if (Regex.IsMatch(content, AmIPidorPattern, RegexOptions.IgnoreCase))
                    await AmIPidor(userMessage);
                else if (Regex.IsMatch(content, GuildPidorTopPattern, RegexOptions.IgnoreCase))
                    await GuildPidorTop(userMessage);
                else
                    await userMessage.DeleteAsync();
            });

            return Task.CompletedTask;
        }

        private async Task GuildPidorTop(SocketUserMessage userMessage)
        {
            var context = new SocketCommandContext(_client, userMessage);
            var top = await _pidorStore.GetGuildTop(context.Guild.Id);

            if (top.Count == 0)
            {
                await context.Channel.SendMessageAsync("Все чисто. Пидоры не обнаружены.");
                return;
            }

            string GetTimesString(int count)
            {
                var rem = count % 10;
                count %= 100;
                if (rem > 1 && rem < 5 && (count < 10 || count > 20))
                    return "раза";
                return "раз";
            }

            var result = string.Join(Environment.NewLine, top.Where(t => context.Guild.GetUser(t.User) != null).Select(t =>
            {
                var user = context.Guild.GetUser(t.User);
                var name = $"{user.Nickname ?? user.Username}{(user.Nickname == null ? "" : $" ({user.Username})")}";
                return $"**{name}** замечен  среди пидоров {t.Count} {GetTimesString(t.Count)}.";
            }));
            await context.Channel.SendMessageAsync(result);
        }

        private async Task AmIPidor(SocketUserMessage userMessage)
        {
            var context = new SocketCommandContext(_client, userMessage);
            if (await _pidorStore.AddGuildPidorParticipant(context.Guild.Id, userMessage.Author.Id))
                await userMessage.Channel.SendMessageAsync($"<@{userMessage.Author.Id}>, я проверю обязательно, как только будет время!");
            else
                await userMessage.Channel.SendMessageAsync($"<@{userMessage.Author.Id}>, скоро узнаем...");
        }

        private async Task WhoIsPidor(SocketUserMessage userMessage)
        {
            if (_pidorSearchActive)
                return;

            _pidorSearchActive = true;
            try
            {
                var context = new SocketCommandContext(_client, userMessage);
                var guildPidor = await _pidorStore.GetCurrentGuildPidor(context.Guild.Id);
                var role = context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(DefaultPidorRole, StringComparison.OrdinalIgnoreCase));
                SocketGuildUser user = null;
                if (guildPidor != null)
                {
                    user = context.Guild.GetUser(guildPidor.Value);

                    if (user != null)
                    {
                        await userMessage.Channel.SendMessageAsync($"A пидор сегодня - **{user.Nickname ?? user.Username}{(user.Nickname != null ? $" ({user.Username})" : "")}**.");
                    }
                    else
                    {
                        await _pidorStore.RemoveGuildPidorParticipant(context.Guild.Id, guildPidor.Value);
                        await userMessage.Channel.SendMessageAsync("Вот пидор, сбежал!");
                    }
                    return;
                }

                var guildParticipants = await _pidorStore.ListGuildPidorParticipants(context.Guild.Id);
                if (guildParticipants.Count == 0)
                {
                    await userMessage.Channel.SendMessageAsync("Все чисто. Пидоры не обнаружены.");
                    return;
                }

                while (user == null)
                {
                    var nextPidor = _rng.Next(0, guildParticipants.Count);
                    var pretended = guildParticipants.ElementAt(nextPidor);
                    user = context.Guild.Users.FirstOrDefault(u => u.Id == pretended);
                    if (user != null)
                        continue;
                    guildParticipants.Remove(pretended);
                    await _pidorStore.RemoveGuildPidorParticipant(context.Guild.Id, pretended);
                }

                var pidorOfTheDayExpires = await _pidorStore.SetGuildPidor(context.Guild.Id, user.Id);
                
                await userMessage.Channel.SendMessageAsync("Начинаю поиск пидора...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Анализирую коренных жителей Починок...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Анализирую понаехов...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Нашел!");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync($"И пидор сегодня - <@{user.Id}>!");
                
                if (role != null)
                {
                    _logger.Information("Added role \"{0}\" to user \"{1}\" at server \"{2}\".", role.Name, user.Username, context.Guild.Name);
                    await user.AddRoleAsync(role, new RequestOptions {AuditLogReason = "Пидор дня!"});
                    _backgroundJobClient.Schedule(() => _removeRoleJob.RemoveRole(context.Guild.Id, user.Id, role.Id, "Больше не пидор дня!"), pidorOfTheDayExpires);
                }
            }
            finally
            {
                _pidorSearchActive = false;
            }
        }

        private async Task GetStats(SocketMessage msg)
        {
            var context = new SocketCommandContext(_client, msg as SocketUserMessage);
            var stat = await _rouletteStore.GetGuildStats(context.Guild.Id);

            await _client.DownloadUsersAsync(new IGuild[] {context.Guild});

            RestUserMessage reply;
            if (stat.Count == 0)
                reply = await msg.Channel.SendMessageAsync($"Пусто... сыграйте \"рулетка!\"");
            else
            {
                reply = await msg.Channel.SendMessageAsync(string.Join(Environment.NewLine,
                    stat.Select(s =>
                    {
                        var (key, (wins, loses)) = s;
                        var user = _client.GetUser(key);
                        return $"{user.Username}: winrate={100.0 * wins / (wins + loses):f1}%";
                    })));
            }

            await DeleteMessages(new IDeletable[] {reply, msg }, 10);
        }

        private async Task PlayRoulette(SocketUserMessage userMessage)
        {
            var context = new SocketCommandContext(_client, userMessage);
            var value = _rng.Next(0, 6);
            string result;

            if (value == 0)
            {
                result = $"**ВЫСТРЕЛ КОЛЬТА В ТУПОЕ ЕБЛО <@{userMessage.Author.Id}> РАЗНОСИТ МОЗГ ПО ДРОБЯМ!**";
                await _rouletteStore.IncrementRouletteWins(context.Guild.Id, userMessage.Author.Id);
            }
            else
            {
                result = "*Сегодня тебя пронесло, ковбой.*";
                await _rouletteStore.IncrementRouletteLoses(context.Guild.Id, userMessage.Author.Id);
            }
            
            var reply = await userMessage.Channel.SendMessageAsync(result);
            await DeleteMessages(new IDeletable[] { userMessage, reply }, 10);
        }

        private static Task DeleteMessages(IReadOnlyCollection<IDeletable> messages, int delaySeconds = 0)
        {
            if (messages.Count == 0)
                return Task.CompletedTask;

            if (delaySeconds > 0)
            {
                return Task.Delay(1000 * delaySeconds)
                        .ContinueWith(t => Task.WhenAll(messages.Select(m => m.DeleteAsync())));
            }

            return Task.WhenAll(messages.Select(m => m.DeleteAsync()));
        }
    }
}