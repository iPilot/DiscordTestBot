using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Hangfire;
using Hangfire.AspNetCore;
using Hangfire.Redis;
using Microsoft.Extensions.DependencyInjection;
using PochinkiBot.Background;
using PochinkiBot.Client;
using PochinkiBot.Client.Commands;
using PochinkiBot.Client.Commands.DailyPidor.Scripts;
using PochinkiBot.Configuration;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Implementations;
using PochinkiBot.Repositories.Interfaces;
using Serilog;
using Serilog.Events;

namespace PochinkiBot
{
    public static class Program
    {
        private const string DefaultConfigFileName = "config.json";
        private const string ConfigArgKey = "--c";

        public static async Task Main(string[] args)
        {
            var provider = ConfigureServiceProvider(args);
            var redis = provider.GetRequiredService<IRedisConnectionProvider>();
            var config = provider.GetRequiredService<BotConfig>();

            if (!await redis.TryConnect())
            {
                Console.WriteLine("Redis is not available.");
                return;
            }

            GlobalConfiguration.Configuration
                .UseDefaultActivator()
                .UseRedisStorage(redis.Connection,
                    new RedisStorageOptions
                    {
                        Db = config.RedisConfiguration.JobStorageDatabase
                    })
                .UseSerilogLogProvider()
                .UseActivator(new AspNetCoreJobActivator(provider.GetRequiredService<IServiceScopeFactory>()));

            var jobHandler = provider.GetRequiredService<BackgroundJobHandler>();
            jobHandler.Run();

            var client = provider.GetRequiredService<DiscordClient>();
            await client.WaitForShutdown();
        }

        private static IServiceProvider ConfigureServiceProvider(string[] args)
        {
            var configFileName = GetConfigFileNameFromArgs(args);
            var services = new ServiceCollection();
            services.AddSingleton(new BotConfig(configFileName));
            services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();
            services.AddSingleton<IRedisDatabaseProvider, RedisDatabaseProvider>();
            services.AddSingleton<DiscordClient>();
            services.AddSingleton<IBackgroundJobClient, BackgroundJobClient>();
            services.AddSingleton<ICommandCollection, CommandCollection>();
            services.AddSingleton<ICommandArgsParser, CommandArgsParser>();
            
            services.Scan(s => s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IRedisStore>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

            services.Scan(s => s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IBackgroundJob>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());

            services.Scan(s => s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IBotCommand>().WithAttribute<CommandAttribute>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());

            services.Scan(s => s.FromApplicationDependencies()
                .AddClasses(c => c.AssignableTo<IDailyPidorScript>())
                .AsSelfWithInterfaces()
                .WithSingletonLifetime());

            services.AddSingleton<IBotDeveloperProvider, BotDeveloperProvider>();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
                .WriteTo.Console()
                .CreateLogger();

            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 250
            }));

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