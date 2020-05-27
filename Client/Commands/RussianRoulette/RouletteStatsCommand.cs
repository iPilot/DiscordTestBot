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

            var countArg = argsPos >= userMessage.Content.Length ? "" : userMessage.Content.Substring(argsPos + 1);
            if (string.IsNullOrWhiteSpace(countArg))
                countArg = "5";

            string message;
            if (!int.TryParse(countArg, out var count))
            {
                await userMessage.DeleteAsync();
                return;
            }

            count = Math.Max(5, count);
            if (stat.Count == 0)
                message = $"Пусто... сыграйте \"{_client.CurrentUser.Mention} рулетка!\"";
            else
            {
                var z = stat
                    .Select(s => (s.Value, context.Guild.GetUser(s.Key)))
                    .Where(s => s.Item2 != null)
                    .OrderByDescending(s => Math.Round((double)s.Value.Wins / (s.Value.Wins + s.Value.Loses), 1))
                    .ThenByDescending(s => s.Value.Wins+s.Value.Loses)
                    .ThenByDescending(s => s.Value.Wins)
                    .ThenBy(s => s.Item2.Id)
                    .Take(count)
                    .Select(s =>
                    {
                        var ((wins, loses), user) = s;
                        var c = wins + loses;
                        return $"**{user.GetUserDisplayName()}** застрелился {wins} {wins.ToTimesString()}! ({100.0 * wins / c:f1}% от {c})";
                    });
                message = string.Join(Environment.NewLine, z);
            }

            var reply = await userMessage.Channel.SendMessageAsync(message);
            await MessageUtilities.DeleteMessagesAsync(15, userMessage, reply);
        }
    }
}