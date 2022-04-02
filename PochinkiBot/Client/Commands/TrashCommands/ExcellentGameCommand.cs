using System.Threading.Tasks;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.TrashCommands
{
    [Command("охуенная игра?")]
    public class ExcellentGameCommand : IBotCommand
    {
        public Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            return userMessage.Channel.SendMessageAsync("https://i.imgur.com/YUMQAmt.jpg");
        }
    }
}