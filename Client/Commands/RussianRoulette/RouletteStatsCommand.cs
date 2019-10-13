using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Client.Commands.RussianRoulette
{
    [Command("ковбои")]
    public class RouletteStatsCommand : IBotCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly IRouletteStore _rouletteStore;

        public RouletteStatsCommand(DiscordSocketClient client, IRouletteStore rouletteStore)
        {
            _client = client;
            _rouletteStore = rouletteStore;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var context = userMessage.GetMessageContext(_client);
            var stat = await _rouletteStore.GetGuildStats(context.Guild.Id, 10);

            string message;
            if (stat.Count == 0)
                message = $"Пусто... сыграйте \"@bot рулетка!\"";
            else
            {
                var z = stat
                    .Select(s => (s.Value, context.Guild.GetUser(s.Key)))
                    .Where(s => s.Item2 != null)
                    .Select(s =>
                    {
                        var ((wins, loses), user) = s;
                        return $"**{user.GetUserDisplayName()}** застрелился {wins} {wins.ToTimesString()}! ({100.0 * wins / (wins + loses):f1}%)";
                    });
                message = string.Join(Environment.NewLine, z);
            }

            var reply = await userMessage.Channel.SendMessageAsync(message);
            await MessageUtilities.DeleteMessagesAsync(10, userMessage, reply);
        }
    }
}