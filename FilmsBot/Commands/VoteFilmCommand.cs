using Discord;
using Discord.WebSocket;
using FilmsBot.Commands.Abstractions;

namespace FilmsBot.Commands
{
    public class VoteFilmCommand : SlashSubCommandBase<FilmsCommand>
    {
        public override string Name => "голос";
        protected override string Description => "Отдать сумму \"денег\" за указанный фильм";

        protected override SlashCommandOptionBuilder ConfigureSubCommand(SlashCommandOptionBuilder builder)
        {
            return builder
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("фильм")
                    .WithDescription("Имя фильма")
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("сумма")
                    .WithDescription("Сумма денег")
                    .WithType(ApplicationCommandOptionType.Integer)
                    .WithRequired(true)
                    .WithMinValue(0)
                    .WithMaxValue(1000));
        }

        public override Task HandleCommand(SocketSlashCommand command, SocketSlashCommandDataOption options)
        {
            return command.RespondAsync(Name);
        }
    }
}