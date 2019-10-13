using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using PochinkiBot.Background.Jobs;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;
using Serilog;

namespace PochinkiBot.Client.Commands.DailyPidor
{
    [Command("кто пидор?")]
    public sealed class WhoIsPidorCommand : IBotCommand
    {
        private const string DefaultPidorRole = "пидор дня";

        private readonly DiscordSocketClient _client;
        private readonly IPidorStore _pidorStore;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRemoveRoleJob _removeRoleJob;
        private bool _pidorSearchActive;
        private readonly ILogger _logger = Log.Logger;
        

        public WhoIsPidorCommand(DiscordSocketClient client, IPidorStore pidorStore, IBackgroundJobClient backgroundJobClient, IRemoveRoleJob removeRoleJob)
        {
            _client = client;
            _pidorStore = pidorStore;
            _backgroundJobClient = backgroundJobClient;
            _removeRoleJob = removeRoleJob;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
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
                        await userMessage.Channel.SendMessageAsync($"A пидор сегодня - **{user.GetUserDisplayName()}**.");
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

                var rng = new Random((int)DateTime.UtcNow.Ticks);
                while (user == null)
                {
                    var nextPidor = rng.Next(0, guildParticipants.Count);
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
                await userMessage.Channel.SendMessageAsync($"И пидор сегодня - {user.Mention}!");
                
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
    }
}