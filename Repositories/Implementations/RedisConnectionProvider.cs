using System;
using System.Threading.Tasks;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;
using Serilog;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Implementations
{
    public class RedisConnectionProvider : IRedisConnectionProvider
    {
        private readonly BotConfig _config;
        public ConnectionMultiplexer Connection { get; private set; }
        private readonly ILogger _logger = Log.Logger;
        
        public RedisConnectionProvider(BotConfig config)
        {
            _config = config;
        }

        public async Task<bool> TryConnect()
        {
            if (Connection != null && Connection.IsConnected)
                return true;

            try
            {
                Connection = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions
                {
                    EndPoints = { _config.RedisConfiguration.Host },
                    DefaultDatabase = _config.RedisConfiguration.Database,
                    SyncTimeout = _config.RedisConfiguration.Timeout
                });

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                return false;
            }
        }
    }
}