using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Repositories.Interfaces;

namespace DiscordBot.Repositories.Implementations
{
    public class RouletteRedisStore : IRouletteStore
    {
        private static string GetGuildWinsStatsKey(ulong guildId) => $"RouletteStats:{guildId}:Wins";
        private static string GetGuildLosesStatsKey(ulong guildId) => $"RouletteStats:{guildId}:Loses";

        private readonly IRedisDatabaseProvider _redisDatabaseProvider;

        public RouletteRedisStore(IRedisDatabaseProvider redisDatabaseProvider)
        {
            _redisDatabaseProvider = redisDatabaseProvider;
        }

        public Task IncrementRouletteLoses(ulong guildId, ulong userId)
        {
            return _redisDatabaseProvider.Database.HashIncrementAsync(GetGuildLosesStatsKey(guildId), userId);
        }

        public Task IncrementRouletteWins(ulong guildId, ulong userId)
        {
            return _redisDatabaseProvider.Database.HashIncrementAsync(GetGuildWinsStatsKey(guildId), userId);
        }

        public async Task<Dictionary<ulong, (int Wins, int Loses)>> GetGuildStats(ulong guildId)
        {
            var userWins = await _redisDatabaseProvider.Database.HashGetAllAsync(GetGuildWinsStatsKey(guildId));
            var userLoses = await _redisDatabaseProvider.Database.HashGetAllAsync(GetGuildLosesStatsKey(guildId));

            var stat = new Dictionary<ulong, (int Wins, int Loses)>();

            foreach (var win in userWins)
            {
                var user = (ulong) win.Name;
                stat[user] = (Wins: (int) win.Value, 0);
            }

            foreach (var lose in userLoses)
            {
                var user = (ulong) lose.Name;
                var wins = stat.TryGetValue(user, out var c) ? c.Wins : 0;
                stat[user] = (wins, (int) lose.Value);
            }

            return stat;
        }
    }
}