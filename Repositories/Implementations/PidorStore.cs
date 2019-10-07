using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PochinkiBot.Repositories.Interfaces;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Implementations
{
    public class PidorStore : IPidorStore
    {
        private readonly IRedisDatabaseProvider _redisDatabase;
        private static string GuildCurrentPidorKey(ulong guildId) => $"Pidors:Current:{guildId}";
        private static string GuildPidorStatsKey(ulong guildId) => $"Pidors:Stats:{guildId}";
        private static string GuildPidorParticipants(ulong guildId) => $"Pidors:Participants:{guildId}";

        public PidorStore(IRedisDatabaseProvider redisDatabase)
        {
            _redisDatabase = redisDatabase;
        }

        public async Task<ulong?> GetCurrentGuildPidor(ulong guildId)
        {
            var current = await _redisDatabase.Database.StringGetAsync(GuildCurrentPidorKey(guildId));
            return current.HasValue 
                ? (ulong) current 
                : (ulong?) null;
        }

        public async Task SetGuildPidor(ulong guildId, ulong userId)
        {
            var now = DateTime.UtcNow.Date;
            await _redisDatabase.Database.StringSetAsync(GuildCurrentPidorKey(guildId), userId, now.AddDays(1).AddHours(7) - DateTime.UtcNow);
            await _redisDatabase.Database.HashIncrementAsync(GuildPidorStatsKey(guildId), userId);
        }

        public Task<bool> AddGuildPidorParticipant(ulong guildId, ulong userId)
        {
            return _redisDatabase.Database.HashSetAsync(GuildPidorParticipants(guildId), userId, DateTime.UtcNow.ToString("O"), When.NotExists);
        }

        public Task RemoveGuildPidorParticipant(ulong guildId, ulong userId)
        {
            return _redisDatabase.Database.HashDeleteAsync(GuildPidorParticipants(guildId), userId);
        }

        public async Task<HashSet<ulong>> ListGuildPidorParticipants(ulong guildId)
        {
            var participants = await _redisDatabase.Database.HashKeysAsync(GuildPidorParticipants(guildId));
            return participants.Select(p => (ulong) p).ToHashSet();
        }

        public async Task<List<(ulong User, int Count)>> GetGuildTop(ulong guildId)
        {
            var top = await _redisDatabase.Database.HashGetAllAsync(GuildPidorStatsKey(guildId));
            return top
                .Select(e => (User: (ulong) e.Name, Count: (int) e.Value))
                .OrderByDescending(p => p.Count)
                .ThenBy(p => p.User)
                .Take(10)
                .ToList();
        }
    }
}