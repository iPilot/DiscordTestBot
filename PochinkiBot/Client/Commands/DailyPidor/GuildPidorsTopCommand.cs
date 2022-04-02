using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Client.Commands.DailyPidor
{
    [Command("перепись пидоров")]
    [CommandArg("count", typeof(int), 10)]
    public sealed class GuildPidorsTopCommand : IBotCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly IPidorStore _pidorStore;

        public GuildPidorsTopCommand(DiscordSocketClient client, IPidorStore pidorStore)
        {
            _client = client;
            _pidorStore = pidorStore;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var context = userMessage.GetMessageContext(_client);
            var top = await _pidorStore.GetGuildTop(context.Guild.Id, 10);

            if (top.Count == 0)
            {
                await context.Channel.SendMessageAsync("Все чисто. Пидоры не обнаружены.");
                return;
            }

            var result = string.Join(Environment.NewLine, top.Where(t => context.Guild.GetUser(t.User) != null).Select(t =>
            {
                var user = context.Guild.GetUser(t.User);
                return $"**{user.GetUserDisplayName()}** замечен  среди пидоров {t.Count} {t.Count.ToTimesString()}.";
            }));
            await context.Channel.SendMessageAsync(result);
        }
    }
}