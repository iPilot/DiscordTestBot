using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Implementations
{
    public class PidorStore : IPidorStore, IRedisStore
    {
        private readonly IRedisDatabaseProvider _redisDatabase;
        private readonly BotConfig _configuration;
        private static string GuildCurrentPidorKey(ulong guildId) => $"Pidors:Current:{guildId}";
        private static string GuildPidorStatsKey(ulong guildId) => $"Pidors:Stats:{guildId}";
        private static string GuildPidorParticipants(ulong guildId) => $"Pidors:Participants:{guildId}";

        public PidorStore(IRedisDatabaseProvider redisDatabase, BotConfig configuration)
        {
            _redisDatabase = redisDatabase;
            _configuration = configuration;
        }

        public async Task<ulong?> GetCurrentGuildPidor(ulong guildId)
        {
            var current = await _redisDatabase.Database.StringGetAsync(GuildCurrentPidorKey(guildId));
            return current.HasValue 
                ? (ulong) current 
                : (ulong?) null;
        }

        public async Task<TimeSpan> SetGuildPidor(ulong guildId, ulong userId)
        {
            var now = DateTime.UtcNow;
            var expiration = _configuration.DailyPidor.DurationSeconds > 0 
                ? TimeSpan.FromSeconds(_configuration.DailyPidor.DurationSeconds) 
                : now.Date.AddHours(24 + 7) - now;
            await _redisDatabase.Database.StringSetAsync(GuildCurrentPidorKey(guildId), userId, expiration);
            await _redisDatabase.Database.HashIncrementAsync(GuildPidorStatsKey(guildId), userId);
            return expiration;
        }

        public Task<bool> AddGuildPidorParticipant(ulong guildId, ulong userId)
        {
            return _redisDatabase.Database.HashSetAsync(GuildPidorParticipants(guildId), userId, DateTime.UtcNow.ToString("O"), When.NotExists);
        }

        public Task RemoveGuildPidorParticipant(ulong guildId, ulong userId)
        {
            return _redisDatabase.Database.HashDeleteAsync(GuildPidorParticipants(guildId), userId);
        }

        public async Task<List<ulong>> ListGuildPidorParticipants(ulong guildId)
        {
            var participants = await _redisDatabase.Database.HashKeysAsync(GuildPidorParticipants(guildId));
            return participants.Select(p => (ulong) p).OrderBy(d => d).ToList();
        }

        public async Task<List<(ulong User, int Count)>> GetGuildTop(ulong guildId, int count)
        {
            var top = await _redisDatabase.Database.HashGetAllAsync(GuildPidorStatsKey(guildId));
            return top
                .Select(e => (User: (ulong) e.Name, Count: (int) e.Value))
                .OrderByDescending(p => p.Count)
                .ThenBy(p => p.User)
                .Take(count)
                .ToList();
        }
    }
}