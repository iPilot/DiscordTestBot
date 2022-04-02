using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;
using FilmsBot.Database;
using Microsoft.EntityFrameworkCore;

namespace FilmsBot.Commands
{
    public class AddParticipantCommand : SlashSubCommandBase<FilmsCommand>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public override string Name => "смотреть";
        protected override string Description => "Стать смотрящим фильмы";

        public AddParticipantCommand(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public override async Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FilmsBotDbContext>();
            if (await db.Participants.FindAsync(command.User.Id) != null)
            {
                await command.DeleteOriginalResponseAsync();
                return;
            }

            db.Participants.Add(new Participant
            {
                Id = command.User.Id,
                JoinedAt = DateTime.UtcNow
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                await command.RespondAsync("Db error");
            }

            await command.RespondAsync("ОК");
        }
    }
}