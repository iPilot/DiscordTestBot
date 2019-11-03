using System.Collections.Generic;
using Discord.WebSocket;

namespace PochinkiBot.Client.Commands.DailyPidor.Scripts
{
    public class PoliceStyleDailyPidorScript : IDailyPidorScript
    {
        private static readonly string[] ConstantPhrases = 
        {
            "Поступил вызов, возможно пидор.",
            "Экипажи выехали :police_car: :police_car: :police_car:",
            ":oncoming_police_car: Это департамент полиции Починок. :oncoming_police_car: Вы окружены! :oncoming_police_car:",
            "Выходите с поднятыми руками."
        };

        public IEnumerable<string> GetPhrases(SocketUser target)
        {
            foreach (var phrase in ConstantPhrases)
            {
                yield return phrase;
            }

            yield return $"{target.Mention}, пидор, ты арестован!";
        }
    }
}