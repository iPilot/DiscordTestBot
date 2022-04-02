using System.Threading.Tasks;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands
{
    public interface IBotCommand
    {
        Task Execute(SocketUserMessage userMessage, int argsPos);
    }
}