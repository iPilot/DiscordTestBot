using System.Collections.Generic;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.DailyPidor.Scripts
{
    public class RandomGuyDailyScript : IDailyPidorScript
    {
        private static readonly string[] ConstantPhrases = 
        {
            "Шел вчера по улице и вижу человека в одной майке",
            "На которой написано: \"Я не пидор, но $20 - это $20!\"",
            "Без штанов ходит, размахивая хером",
        };

        public IEnumerable<string> GetPhrases(SocketUser target)
        {
            foreach (var phrase in ConstantPhrases)
            {
                yield return phrase;
            }

            yield return $"Присмотрелся, а это - {target.Mention}!";
        }
    }
}