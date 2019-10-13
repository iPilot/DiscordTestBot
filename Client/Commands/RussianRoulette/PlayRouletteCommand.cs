using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using PochinkiBot.Background.Jobs;
using PochinkiBot.Configuration;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Client.Commands.RussianRoulette
{
    [Command("рулетка!")]
    public class PlayRouletteCommand : IBotCommand
    {
        private const string DefaultRouletteRole = "ковбой";

        private readonly DiscordSocketClient _client;
        private readonly IRouletteStore _rouletteStore;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly BotConfig _config;
        private readonly IRemoveRoleJob _removeRoleJob;
        
        public PlayRouletteCommand(DiscordSocketClient client,
            IRouletteStore rouletteStore,
            IBackgroundJobClient backgroundJobClient,
            BotConfig config,
            IRemoveRoleJob removeRoleJob)
        {
            _client = client;
            _rouletteStore = rouletteStore;
            _backgroundJobClient = backgroundJobClient;
            _config = config;
            _removeRoleJob = removeRoleJob;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var context = new SocketCommandContext(_client, userMessage);
            var role = context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(DefaultRouletteRole, StringComparison.OrdinalIgnoreCase));
            if (role == null || role.Permissions.SendMessages)
            {
                await userMessage.DeleteAsync();
                return;
            }

            if (await _rouletteStore.HasUserRouletteCooldown(context.Guild.Id, userMessage.Author.Id))
            {
                var reply = await userMessage.Channel.SendMessageAsync("*Не так часто, ковбой!*");
                await MessageUtilities.DeleteMessagesAsync(5, reply, userMessage);
                return;
            }

            var value = new Random((int)DateTime.UtcNow.Ticks).Next(0, 6);
            string result;
            if (value == 0)
            {
                result = $"**ВЫСТРЕЛ КОЛЬТА В ТУПОЕ ЕБЛО <@{userMessage.Author.Id}> РАЗНОСИТ МОЗГ ПО ДРОБЯМ!**";
                await _rouletteStore.IncrementRouletteWins(context.Guild.Id, userMessage.Author.Id);
                if (userMessage.Author is SocketGuildUser user)
                {
                    await user.AddRoleAsync(role, new RequestOptions {AuditLogReason = "Застрелился!"});
                    var expiry = TimeSpan.FromSeconds(_config.RussianRoulette.RouletteCooldownSeconds);
                    _backgroundJobClient.Schedule(() => _removeRoleJob.RemoveRole(context.Guild.Id, user.Id, role.Id, "Жив, цел, орёл!"), expiry);
                }
            }
            else
            {
                result = "*Сегодня тебя пронесло, ковбой.*";
                await _rouletteStore.IncrementRouletteLoses(context.Guild.Id, userMessage.Author.Id);
            }

            await _rouletteStore.SetUserRouletteCooldown(context.Guild.Id, userMessage.Author.Id);
            await userMessage.Channel.SendMessageAsync(result);
        }
    }
}