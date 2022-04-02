using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace PochinkiBot.Misc
{
    public static class MessageUtilities
    {
        public static Task DeleteMessagesAsync(int delaySeconds = 0, params IDeletable[] messages)
        {
            var delay = Math.Max(delaySeconds, 0);
            return delaySeconds == 0 
                ? Task.WhenAll(messages.Select(m => m.DeleteAsync()))
                : Task.Delay(TimeSpan.FromSeconds(delay)).ContinueWith(t => Task.WhenAll(messages.Select(m => m.DeleteAsync())));
        }

        public static string ToTimesString(this int count)
        {
            var rem = count % 10;
            count %= 100;
            if (rem > 1 && rem < 5 && (count < 10 || count > 20))
                return "раза";
            return "раз";
        }

        public static string FormatForMessage(this TimeSpan time)
        {
            if (time.Days > 0 || time.Hours > 0)
                return $" **(еще {time.TotalHours:F1} часов)**";

            return $" **(еще {time.TotalMinutes:F1} минут)**";
        }
    }
}