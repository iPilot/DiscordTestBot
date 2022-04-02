#nullable disable
using Discord;
using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;
using FilmsBot.Database;
using FilmsBot.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FilmsBot.Commands
{
    public class AddFilmSubCommand : DbInteractionSubCommand<FilmsCommand, FilmsBotDbContext>
    {
        public override string Name => "добавить";
        protected override string Description => "Добавление нового фильма в список";

        public AddFilmSubCommand(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        protected override async Task<string> HandleCommandInternal(IServiceScope scope, FilmsBotDbContext db, SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            var name = options.GetOptionValue<string>("название").Trim();
            var year = options.GetOptionValue<int?>("год");

            if (await db.Films.AnyAsync(f => f.Name == name && f.Year == year))
                return "Уже добавлено";

            var user = await db.Participants.FindAsync(command.User.Id);
            if (user == null)
            {
                user = new Participant
                {
                    Id = command.User.Id,
                    JoinedAt = DateTime.UtcNow
                };
                db.Participants.Add(user);
            }

            var film = new Film
            {
                GuildId = command.Channel is IGuildChannel g ? g.GuildId : null,
                Name = name,
                AddedBy = user,
                Year = year
            };

            db.Films.Add(film);

            return null;
        }

        protected override SlashCommandOptionBuilder ConfigureSubCommand(SlashCommandOptionBuilder builder)
        {
            return builder
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("название")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithDescription("Название фильма")
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("год")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithMinValue(1900)
                    .WithMaxValue(2100)
                    .WithRequired(false)
                    .WithDescription("Год выхода фильма"));
        }
    }
}