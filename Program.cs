using System;
using System.Threading.Tasks;
using DiscordBot.Client;
using DiscordBot.Configuration;
using DiscordBot.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var provider = ConfigureServiceProvider();
            await provider.GetRequiredService<DiscordClient>().Run();
        }

        private static IServiceProvider ConfigureServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new BotConfig());
            services.AddSingleton<IRedisStorage, RedisStorage>();
            services.AddSingleton<DiscordClient>();

            return services.BuildServiceProvider();
        }
    }
}