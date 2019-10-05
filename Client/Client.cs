using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Configuration;
using DiscordBot.Repositories;

namespace DiscordBot.Client
{
    public class DiscordClient
    {
        private readonly BotConfig _config;
        private readonly IRedisStorage _redisStorage;
        private readonly DiscordSocketClient _client;

        public DiscordClient(BotConfig config, IRedisStorage redisStorage)
        {
            _config = config;
            _redisStorage = redisStorage;
            _client = new DiscordSocketClient();
        }

        public async Task Run()
        {
            using (_client)
            {
                ConfigureEvents();

                await _client.LoginAsync(TokenType.Bot, _config.Token);
                await _client.StartAsync();

                if (_client.ConnectionState == ConnectionState.Connected)
                    await _redisStorage.Storage.StringSetAsync("StartedAt", DateTime.UtcNow.ToString("O"));
                
                await Task.Delay(-1);
            }
        }

        private void ConfigureEvents()
        {
            _client.Log += Log;
            _client.Ready += () => Log(new LogMessage(LogSeverity.Info, nameof(_client), "Ready to work!"));

            _client.MessageReceived += OnClientOnMessageReceived;
        }

        private Task OnClientOnMessageReceived(SocketMessage msg)
        {
            return msg.Author.Equals(_client.CurrentUser)
                ? Task.CompletedTask
                : msg.Channel.SendMessageAsync($"Received: {msg.Content} from {msg.Author.Username}");
        }

        private static Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}