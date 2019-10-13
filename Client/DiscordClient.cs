using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PochinkiBot.Client.Commands;
using PochinkiBot.Configuration;
using PochinkiBot.Repositories.Interfaces;
using Serilog;

namespace PochinkiBot.Client
{
    public class DiscordClient
    {
        private readonly BotConfig _config;
        private readonly IRedisDatabaseProvider _redisDatabaseProvider;
        private readonly ICommandCollection _commandCollection;
        private readonly DiscordSocketClient _client;
        private readonly ILogger _logger = Log.Logger;
        
        public DiscordClient(BotConfig config,
            IRedisDatabaseProvider redisDatabaseProvider,
            ICommandCollection commandCollection,
            DiscordSocketClient client)
        {
            _config = config;
            _redisDatabaseProvider = redisDatabaseProvider;
            _commandCollection = commandCollection;
            _client = client;
        }

        public async Task WaitForShutdown()
        {
            using (_client)
            {
                ConfigureEvents();

                await _client.LoginAsync(TokenType.Bot, _config.Token);
                await _client.StartAsync();

                await Task.Delay(-1);
            }
        }

        private void ConfigureEvents()
        {
            _client.Log += msg =>
            {
                var logger = _logger.ForContext("Source", msg.Source);
                switch (msg.Severity)
                {
                    case LogSeverity.Critical:
                        logger.Fatal(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Error:
                        logger.Error(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Warning:
                        logger.Warning(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Info:
                        logger.Information(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Verbose:
                        logger.Verbose(msg.Exception, msg.Message);
                        break;
                    case LogSeverity.Debug:
                        logger.Debug(msg.Exception, msg.Message);
                        break;
                }
                return Task.CompletedTask;
            };
            _client.Ready += () => _redisDatabaseProvider.Database.StringSetAsync("StartedAt", DateTime.UtcNow.ToString("O"));
            _client.MessageReceived += OnClientOnMessageReceived;
        }

        private Task OnClientOnMessageReceived(SocketMessage msg)
        {
            Task.Run(async () =>
            {
                if (!(msg is SocketUserMessage userMessage))
                    return;

                if (userMessage.Channel is IDMChannel)
                    return;

                var pos = 0;
                if (!userMessage.HasMentionPrefix(_client.CurrentUser, ref pos))
                    return;

                var command = _commandCollection.GetCommand(msg.Content, pos);

                if (command != null)
                    await command.Execute(userMessage, pos);
                else
                    await userMessage.DeleteAsync();
            });

            return Task.CompletedTask;
        }
    }
}