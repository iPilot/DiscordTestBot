using System.Threading.Tasks;
using PochinkiBot.Repositories.Interfaces;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Implementations
{
    public class PastaStore : IPastaStore, IRedisStore
    {
        private readonly IRedisDatabaseProvider _redis;
        private RedisKey PastaKey(ulong guildId, string name) => $"Pasta:{guildId}:{name}";

        public PastaStore(IRedisDatabaseProvider redis)
        {
            _redis = redis;
        }

        public async Task<string> GetPasta(ulong serverId, string name)
        {
            var key = PastaKey(serverId, name);
            return await _redis.Database.StringGetAsync(key);
        }

        public Task SavePasta(ulong serverId, string name, string content)
        {
            var key = PastaKey(serverId, name);

            return _redis.Database.StringSetAsync(key, content);
        }
    }
}