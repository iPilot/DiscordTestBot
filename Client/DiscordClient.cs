using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Configuration;
using DiscordBot.Repositories.Interfaces;

namespace DiscordBot.Client
{
    public class DiscordClient
    {
        private readonly BotConfig _config;
        private readonly IRedisDatabaseProvider _redisDatabaseProvider;
        private readonly IRouletteStore _rouletteStore;
        private readonly IPidorStore _pidorStore;
        private readonly DiscordSocketClient _client;

        private const string RoulettePattern = @"рулетка!$";
        private const string StatsPattern = @"статистика!$";
        private const string WhoPidorPattern = @"кто пидор?";
        private const string AmIPidorPattern = @"я пидор?";
        private const string GuildPidorTopPattern = @"перепись пидоров";

        private bool _pidorSearchActive;

        private readonly Random _rng = new Random((int)DateTime.UtcNow.Ticks);

        public DiscordClient(BotConfig config, IRedisDatabaseProvider redisDatabaseProvider, IRouletteStore rouletteStore, IPidorStore pidorStore)
        {
            _config = config;
            _redisDatabaseProvider = redisDatabaseProvider;
            _rouletteStore = rouletteStore;
            _pidorStore = pidorStore;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 250
            });
        }

        public async Task Run()
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
            _client.Log += Log;
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

                if (Regex.IsMatch(content, RoulettePattern))
                    await PlayRoulette(userMessage);

                if (Regex.IsMatch(content, StatsPattern))
                    await GetStats(userMessage);

                if (Regex.IsMatch(content, WhoPidorPattern))
                    await WhoIsPidor(userMessage);

                if (Regex.IsMatch(content, AmIPidorPattern))
                    await AmIPidor(userMessage);

                if (Regex.IsMatch(content, GuildPidorTopPattern))
                    await GuildPidorTop(userMessage);
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
                return $"**{user?.Nickname ?? user?.Username ?? t.User.ToString()}** замечен среди пидоров {t.Count} {GetTimesString(t.Count)}.";
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
                SocketGuildUser user = null;
                if (guildPidor != null)
                {
                    user = context.Guild.GetUser(guildPidor.Value);

                    if (user != null)
                    {
                        await userMessage.Channel.SendMessageAsync($"Пидор сегодня - **{user.Nickname ?? user.Username}**.");
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

                await _pidorStore.SetGuildPidor(context.Guild.Id, user.Id);
                await userMessage.Channel.SendMessageAsync("Начинаю поиск пидора...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Анализирую коренных жителей Починок...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Анализирую понаехов...");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync("Нашел!");
                await Task.Delay(1500);
                await userMessage.Channel.SendMessageAsync($"И пидор сегодня - <@{user.Id}>!");

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

        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}