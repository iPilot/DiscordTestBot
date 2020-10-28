using System;
using System.Threading.Tasks;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Repositories.Implementations
{
    public class RedisBirthDateStore : IBirthDayStore, IRedisStore
    {
        private const string BirthDateKey = "BirthDates";
        private readonly IRedisDatabaseProvider _redis;

        public RedisBirthDateStore(IRedisDatabaseProvider redis)
        {
            _redis = redis;
        }

        public Task SaveBirthDate(ulong userId, DateTime birthDate)
        {
            return _redis.Database.HashSetAsync(BirthDateKey, userId, birthDate.ToString("yyyy-MM-dd"));
        }

        public async Task<DateTime?> GetBirthDate(ulong userId)
        {
            var v = await _redis.Database.HashGetAsync(BirthDateKey, userId);
            return v.HasValue && DateTime.TryParse(v, out var date) ? date : (DateTime?) null;
        }
    }
}