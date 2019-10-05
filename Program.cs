using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var provider = ConfigureServiceProvider();
            await provider.GetRequiredService<Client>().Run();
        }

        private static IServiceProvider ConfigureServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new BotConfig());
            services.AddSingleton<IRedisStorage, RedisStorage>();

            return services.BuildServiceProvider();
        }
    }
}