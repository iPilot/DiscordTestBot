using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class FilmsSubCommand : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "все";
        protected override string Description => "Список все добавленных фильмов";

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            return command.RespondAsync(Name);
        }
    }
}