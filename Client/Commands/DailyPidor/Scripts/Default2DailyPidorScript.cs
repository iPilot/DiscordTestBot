using System.Collections.Generic;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.DailyPidor.Scripts
{
    public class Default2DailyPidorScript : IDailyPidorScript
    {
        private static readonly string[] ConstantPhrases = 
        {
            "Начинаю поиск пидора...",
            "Анализирую коренных жителей Починок...",
            "Анализирую понаехов...",
            "Пидор обнаружен!"
        };

        public IEnumerable<string> GetPhrases(SocketUser target)
        {
            foreach (var phrase in ConstantPhrases)
            {
                yield return phrase;
            }

            yield return $"И им оказался {target.Mention}!";
        }
    }
}