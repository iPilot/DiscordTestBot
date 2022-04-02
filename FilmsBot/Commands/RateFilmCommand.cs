using Discord;
using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class RateFilmCommand : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "оценка";
        protected override string Description => "Оценить просмотренный фильм";

        protected override SlashCommandOptionBuilder ConfigureSubCommand(SlashCommandOptionBuilder builder)
        {
            return builder
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("фильм")
                    .WithDescription("Название фильма")
                    .WithType(ApplicationCommandOptionType.String)
                    .WithRequired(true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("оценка")
                    .WithDescription("Оценка фильма")
                    .WithType(ApplicationCommandOptionType.Number)
                    .WithRequired(true)
                    .WithMinValue(0)
                    .WithMaxValue(10));
        }

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            return command.RespondAsync(Name);
        }
    }
}