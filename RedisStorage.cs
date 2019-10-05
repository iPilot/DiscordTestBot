using StackExchange.Redis;

namespace DiscordBot
{
    public class RedisStorage : IRedisStorage
    {
        private readonly ConnectionMultiplexer _connection;

        public RedisStorage(BotConfig config)
        {
            _connection = ConnectionMultiplexer.Connect(config.RedisConnectionString);
        }

        public IDatabase Storage => _connection.GetDatabase();

        // TODO certain methods as for default repository pattern?
    }

    public interface IRedisStorage
    {
        IDatabase Storage { get; }
    }
}