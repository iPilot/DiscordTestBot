using System;
using System.Threading.Tasks;
using DiscordBot.Configuration;
using DiscordBot.Repositories.Interfaces;
using StackExchange.Redis;

namespace DiscordBot.Repositories.Implementations
{
    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        private readonly BotConfig _config;
        private ConnectionMultiplexer _connection;

        public RedisDatabaseProvider(BotConfig config)
        {
            _config = config;
        }
         
        public async Task<bool> TryConnect()
        {
            if (_connection != null && _connection.IsConnected)
                return true;

            try
            {
                _connection = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
                {
                    EndPoints = { _config.RedisConfiguration.Host },
                    DefaultDatabase = _config.RedisConfiguration.Database,
                    SyncTimeout = _config.RedisConfiguration.Timeout
                }, Console.Out);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}. Message: {1}. StackTrace: {2}.", e.GetType().Name, e.Message, e.StackTrace);
                return false;
            }
        }

        public IDatabase Database
        {
            get
            {
                if (!_connection.IsConnected)
                    throw new InvalidOperationException("Not connected.");
                return _connection.GetDatabase();
            }
        }
    }
}