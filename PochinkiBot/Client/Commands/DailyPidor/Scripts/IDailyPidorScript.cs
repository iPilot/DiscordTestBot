using System.Collections.Generic;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.DailyPidor.Scripts
{
    public interface IDailyPidorScript
    {
        IEnumerable<string> GetPhrases(SocketUser target);
    }
}