using System.Collections.Generic;
using System.Threading.Tasks;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IRouletteStore : IRedisStore
    {
        Task IncrementRouletteLoses(ulong guildId, ulong userId);
        Task IncrementRouletteWins(ulong guildId, ulong userId);
        Task<Dictionary<ulong, (int Wins, int Loses)>> GetGuildStats(ulong guildId, int count);
        Task<bool> HasUserRouletteCooldown(ulong guildId, ulong userId);
        Task SetUserRouletteCooldown(ulong guildId, ulong userId);
    }
}