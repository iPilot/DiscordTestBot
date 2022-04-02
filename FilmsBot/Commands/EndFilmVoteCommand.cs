using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class EndFilmVoteCommand : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "стоп";
        protected override string Description => "Завершение голосования за фильмы";

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            return command.RespondAsync(Name);
        }
    }
}