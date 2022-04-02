using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class LoadFilmsData : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "выгрузить";
        protected override string Description => "Выгрузить все фильмы в колесо";

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            
        }
    }
}