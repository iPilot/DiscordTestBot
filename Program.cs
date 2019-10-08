using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PochinkiBot.Background;
using PochinkiBot.Client;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Implementations;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot
{
    public static class Program
    {
        private const string DefaultConfigFileName = "config.json";
        private const string ConfigArgKey = "--c";

        public static async Task Main(string[] args)
        {
            var provider = ConfigureServiceProvider(args);

            var db = provider.GetRequiredService<IRedisDatabaseProvider>();
            if (!await db.TryConnect())
            {
                Console.WriteLine("Redis is not available.");
                return;
            }

            var jobHandler = provider.GetRequiredService<BackgroundJobHandler>();
            jobHandler.Run();

            await provider.GetRequiredService<DiscordClient>().Run();
        }

        private static IServiceProvider ConfigureServiceProvider(string[] args)
        {
            var configFileName = GetConfigFileNameFromArgs(args);
            var services = new ServiceCollection();
            services.AddSingleton(new BotConfig(configFileName));
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.AddSingleton<DiscordClient>();

            services.Scan(s => s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IRedisStore>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.AddScoped<BackgroundJobHandler>();

            var provider = services.BuildServiceProvider();
            services.AddSingleton(provider);
            return provider;
        }

        private static string GetConfigFileNameFromArgs(string[] args)
        {
            var configFile = args.SkipWhile(a => a != ConfigArgKey).Skip(1).FirstOrDefault();
            return configFile == null || !configFile.EndsWith(".json")
                ? DefaultConfigFileName
                : configFile;
        }
    }
}