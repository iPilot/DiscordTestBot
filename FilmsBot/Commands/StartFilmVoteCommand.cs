using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class StartFilmVoteCommand : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "старт";
        protected override string Description => "Начать голосование за следующий фильм";

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            return command.RespondAsync(Name);
        }
    }
}