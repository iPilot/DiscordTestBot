using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using PochinkiBot.Background.Jobs;
using PochinkiBot.Configuration;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;
using Serilog;

namespace PochinkiBot.Client.Commands.RussianRoulette
{
    [Command("рулетка!")]
    public class PlayRouletteCommand : IBotCommand
    {
        private const string DefaultRouletteRole = "ковбой";
        private static readonly string[] WinPhrases =
        {
            "**ВЫСТРЕЛ КОЛЬТА В ТУПОЕ ЕБЛО {0} РАЗНОСИТ МОЗГ ПО ДРОБЯМ!**"
        };
        private static readonly string[] LosePhrases =
        {
            "*Сегодня тебя пронесло, ковбой.*"
        };
        private static readonly string[] CooldownPhrases =
        {
            "*Не так часто, ковбой!*",
            "*Не сейчас.*",
            "*Не торопись помереть!*",
            "*Ты что не видишь? У меня обед!*",
            "*Извини, патроны закончились.*"
        };

        private readonly DiscordSocketClient _client;
        private readonly IRouletteStore _rouletteStore;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IBotDeveloperProvider _botDeveloperProvider;
        private readonly BotConfig _config;
        private readonly IRemoveRoleJob _removeRoleJob;
        private static readonly ILogger Logger = Log.ForContext<PlayRouletteCommand>();

        public PlayRouletteCommand(DiscordSocketClient client,
            IRouletteStore rouletteStore,
            IBackgroundJobClient backgroundJobClient,
            IBotDeveloperProvider botDeveloperProvider,
            BotConfig config,
            IRemoveRoleJob removeRoleJob)
        {
            _client = client;
            _rouletteStore = rouletteStore;
            _backgroundJobClient = backgroundJobClient;
            _botDeveloperProvider = botDeveloperProvider;
            _config = config;
            _removeRoleJob = removeRoleJob;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var context = new SocketCommandContext(_client, userMessage);
            var roleName = _config.RussianRoulette.WinnerRoleName ?? DefaultRouletteRole;
            var role = context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null || role.Permissions.SendMessages)
            {
                await userMessage.DeleteAsync();
                return;
            }

            var rng = new Random((int)DateTime.UtcNow.Ticks);
            var cooldown = await _rouletteStore.UserRouletteCooldown(context.Guild.Id, userMessage.Author.Id);
            if (cooldown > TimeSpan.Zero)
            {
                var phrase = CooldownPhrases[rng.Next(CooldownPhrases.Length)] + cooldown.FormatForMessage();
                var reply = await userMessage.Channel.SendMessageAsync(phrase);
                await MessageUtilities.DeleteMessagesAsync(5, reply, userMessage);
                return;
            }

            var now = DateTime.Now;
            var isAprilFools = now.Day == 1 && now.Month == 4;
            var value = _botDeveloperProvider.IsUserDeveloper(userMessage.Author.Id) ? 1 : isAprilFools ? 6 : rng.Next(0, 600);
            string result;
            if (value % 6 == 0)
            {
                result = isAprilFools ? "**С ПЕРВЫМ АПРЕЛЯ, МУДИЛА!**" : WinPhrases[rng.Next(WinPhrases.Length)];
                await _rouletteStore.IncrementRouletteWins(context.Guild.Id, userMessage.Author.Id);
                if (userMessage.Author is SocketGuildUser user)
                {
                    Logger.Information("Added role \"{0}\" to user \"{1}\" at server \"{2}\".", role.Name, user.Username, context.Guild.Name);
                    await user.AddRoleAsync(role, new RequestOptions {AuditLogReason = "Застрелился!"});
                    var expiry = TimeSpan.FromSeconds(_config.RussianRoulette.WinnerDurationSeconds);
                    _backgroundJobClient.Schedule(() => _removeRoleJob.RemoveRole(context.Guild.Id, user.Id, role.Id, "Жив, цел, орёл!"), expiry);
                    result = string.Format(result, user.Mention);
                }
            }
            else
            {
                result = LosePhrases[rng.Next(LosePhrases.Length)];
                await _rouletteStore.IncrementRouletteLoses(context.Guild.Id, userMessage.Author.Id);
            }

            await _rouletteStore.SetUserRouletteCooldown(context.Guild.Id, userMessage.Author.Id);
            await userMessage.Channel.SendMessageAsync(result);
        }
    }
}