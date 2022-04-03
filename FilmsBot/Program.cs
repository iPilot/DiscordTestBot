using Discord.WebSocket;
using FilmsBot;
using FilmsBot.Client;
using FilmsBot.Commands.Abstractions;
using FilmsBot.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddDbContextPool<FilmsBotDbContext>(o =>
{
    o.UseNpgsql(builder.Configuration["ConnectionString"]);
}, 64);
builder.Services.Scan(s => s.FromApplicationDependencies().AddClasses(a => a.AssignableTo<ISlashCommandHandler>()).AsImplementedInterfaces().WithSingletonLifetime());
builder.Services.Scan(s => s.FromApplicationDependencies().AddClasses(a => a.AssignableTo(typeof(ISlashSubCommandHandler<>))).AsImplementedInterfaces().WithSingletonLifetime());
builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
{
    AlwaysDownloadUsers = true,
    MessageCacheSize = 250
}));
builder.Services.AddSingleton<IBotDeveloperProvider, BotDeveloperProvider>();
builder.Host.UseSerilog((_, c) => c.ReadFrom.Configuration(builder.Configuration));

var app = builder.Build();

var client = app.Services.GetRequiredService<DiscordClient>();

client.Run();
app.Run();