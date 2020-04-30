using System.Collections.Generic;

namespace PochinkiBot.Misc
{
    public interface IBotDeveloperProvider
    {
        bool IsUserDeveloper(ulong userId);
    }

    public class BotDeveloperProvider : IBotDeveloperProvider
    {
        private readonly HashSet<ulong> _developers = new HashSet<ulong>(new ulong[] { 158130734282964993 });

        public bool IsUserDeveloper(ulong userId)
        {
            return _developers.Contains(userId);
        }
    }
}