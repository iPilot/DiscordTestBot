using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace PochinkiBot.Client
{
    public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _serviceProvider;

    public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
    {
        _commands = commands;
        _serviceProvider = serviceProvider;
        _client = client;
    }
    
    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        if (!(messageParam is SocketUserMessage userMessage))
            return;

        var argPos = 0;
        if (!(userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) || userMessage.Author.IsBot)
            return;

        var context = new SocketCommandContext(_client, userMessage);
        await _commands.ExecuteAsync(context, argPos, _serviceProvider);
    }
}
}