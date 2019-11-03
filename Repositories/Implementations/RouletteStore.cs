using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Repositories.Implementations
{
    public class RouletteStore : IRouletteStore
    {
        private static string GuildWinsStatsKey(ulong guildId) => $"RussianRoulette:Stats:{guildId}:Wins";
        private static string GuildLosesStatsKey(ulong guildId) => $"RussianRoulette:Stats:{guildId}:Loses";
        private static string GuildCooldownKey(ulong guildId, ulong userId) => $"RussianRoulette:Cooldown:{guildId}:{userId}";

        private readonly IRedisDatabaseProvider _redisDatabaseProvider;
        private readonly BotConfig _config;

        public RouletteStore(IRedisDatabaseProvider redisDatabaseProvider, BotConfig config)
        {
            _redisDatabaseProvider = redisDatabaseProvider;
            _config = config;
        }

        public Task IncrementRouletteLoses(ulong guildId, ulong userId)
        {
            return _redisDatabaseProvider.Database.HashIncrementAsync(GuildLosesStatsKey(guildId), userId);
        }

        public Task IncrementRouletteWins(ulong guildId, ulong userId)
        {
            return _redisDatabaseProvider.Database.HashIncrementAsync(GuildWinsStatsKey(guildId), userId);
        }

        public async Task<TimeSpan> UserRouletteCooldown(ulong guildId, ulong userId)
        {
            var time = await _redisDatabaseProvider.Database.KeyIdleTimeAsync(GuildCooldownKey(guildId, userId));
            return time ?? TimeSpan.Zero;
        }

        public Task SetUserRouletteCooldown(ulong guildId, ulong userId)
        {
            var expiry = TimeSpan.FromSeconds(_config.RussianRoulette.CooldownSeconds);
            return _redisDatabaseProvider.Database.StringSetAsync(GuildCooldownKey(guildId, userId), "1", expiry);
        }

        public async Task<Dictionary<ulong, (int Wins, int Loses)>> GetGuildStats(ulong guildId, int count)
        {
            var userWins = await _redisDatabaseProvider.Database.HashGetAllAsync(GuildWinsStatsKey(guildId));
            var userLoses = await _redisDatabaseProvider.Database.HashGetAllAsync(GuildLosesStatsKey(guildId));

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