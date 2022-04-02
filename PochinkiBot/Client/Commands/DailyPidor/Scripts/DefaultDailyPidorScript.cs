using System.Collections.Generic;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.DailyPidor.Scripts
{
    public class DefaultDailyPidorScript : IDailyPidorScript
    {
        private static readonly string[] ConstantPhrases = 
        {
            "Начинаю поиск пидора...",
            "Анализирую коренных жителей Починок...",
            "Анализирую понаехов...",
            "Нашел!"
        };

        public IEnumerable<string> GetPhrases(SocketUser target)
        {
            foreach (var phrase in ConstantPhrases)
            {
                yield return phrase;
            }

            yield return $"И пидор сегодня - {target.Mention}!";
        }
    }
}