using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Repositories.Interfaces
{
    public interface IRouletteStore : IRedisStore
    {
        Task IncrementRouletteLoses(ulong guildId, ulong userId);
        Task IncrementRouletteWins(ulong guildId, ulong userId);
        Task<Dictionary<ulong, (int Wins, int Loses)>> GetGuildStats(ulong guildId);
    }
}