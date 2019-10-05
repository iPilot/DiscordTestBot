﻿using Microsoft.Extensions.Configuration;

namespace DiscordBot
{
    public class BotConfig
    {
        private readonly IConfiguration _configuration;

        public string Token { get; }
        public string RedisConnectionString { get; }

        public BotConfig() : this("config.json")
        {
        }

        public BotConfig(string fileName)
        {
            _configuration = new ConfigurationBuilder().AddJsonFile(fileName).Build();
            Token = _configuration["Token"];
            RedisConnectionString = _configuration["Redis"];
        }

        public T Get<T>(string key) => _configuration.GetSection(key).Get<T>();
    }
}