using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordBot
{
    public class Client
    {
        private readonly BotConfig _config;
        private readonly IRedisStorage _redisStorage;

        public Client(BotConfig config, IRedisStorage redisStorage)
        {
            _config = config;
            _redisStorage = redisStorage;
        }

        public async Task Run()
        {
            using (var client = new DiscordSocketClient())
            {
                client.Log += Log;
                client.Ready += () => Log(new LogMessage(LogSeverity.Info, nameof(client), "Ready to work!"));

                await client.LoginAsync(TokenType.Bot, _config.Token);
                await client.StartAsync();
                await _redisStorage.Storage.StringSetAsync("StartedAt", DateTime.UtcNow.ToString("O"));
                
                await Task.Delay(-1);
            }
        }

        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}