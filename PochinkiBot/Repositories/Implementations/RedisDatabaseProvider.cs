using System;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Implementations
{
    public class RedisDatabaseProvider : IRedisDatabaseProvider
    {
        private readonly BotConfig _config;
        private readonly IRedisConnectionProvider _provider;

        public RedisDatabaseProvider(BotConfig config, IRedisConnectionProvider provider)
        {
            _config = config;
            _provider = provider;
        }
         
        public IDatabase Database
        {
            get
            {
                if (!_provider.Connection.IsConnected)
                    throw new InvalidOperationException("Not connected.");
                return _provider.Connection.GetDatabase(_config.RedisConfiguration.Database);
            }
        }
    }
}