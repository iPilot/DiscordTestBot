using System.Threading.Tasks;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.TrashCommands
{
    [Command("помойка?")]
    public class TrashServerCommand : IBotCommand
    {
        public Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            return userMessage.Channel.SendMessageAsync(@"вы похожи на даунов все
диск в помойку превратился
и стример админы только и могут что банить всех просто так
я хуй знает, может это власть так на вас действует
но почитать что было раньше и что сейчас
помойка");
        }
    }
}