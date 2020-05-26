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
        private readonly Random _rng = new Random((int)DateTime.UtcNow.Ticks);

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
            var userId = userMessage.Author.Id;
            var role = context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null || role.Permissions.SendMessages)
            {
                await userMessage.DeleteAsync();
                return;
            }

            var cooldown = await _rouletteStore.UserRouletteCooldown(context.Guild.Id, userId);
            if (cooldown > TimeSpan.Zero)
            {
                var phrase = CooldownPhrases[_rng.Next(CooldownPhrases.Length)] + cooldown.FormatForMessage();
                var reply = await userMessage.Channel.SendMessageAsync(phrase);
                await MessageUtilities.DeleteMessagesAsync(5, reply, userMessage);
                return;
            }

            var (wins, loses) = await _rouletteStore.GetUserStat(context.Guild.Id, userId);
            var now = DateTime.Now;
            var isAprilFools = now.Day == 1 && now.Month == 4;
            var isFirstTime = wins + loses == 0;
            var isDeveloper = _botDeveloperProvider.IsUserDeveloper(userId);
            var value = _rng.GetRouletteNumber(isFirstTime, isDeveloper, isAprilFools);

            var text = await GetText(context, userMessage, value, isAprilFools, role);

            await _rouletteStore.SetUserRouletteCooldown(context.Guild.Id, userMessage.Author.Id);
            await userMessage.Channel.SendMessageAsync(text);
        }

        private async Task<string> GetText(SocketCommandContext context, SocketUserMessage userMessage, int value, bool isAprilFools, SocketRole role)
        {
            if (value % 6 != 0)
            {
                await _rouletteStore.IncrementRouletteLoses(context.Guild.Id, userMessage.Author.Id);
                return LosePhrases[_rng.Next(LosePhrases.Length)];
            }
            
            var template = isAprilFools ? "**С ПЕРВЫМ АПРЕЛЯ, МУДИЛА!**" : WinPhrases[_rng.Next(WinPhrases.Length)];
            await _rouletteStore.IncrementRouletteWins(context.Guild.Id, userMessage.Author.Id);

            if (userMessage.Author is SocketGuildUser user)
            {
                Logger.Information("Added role \"{0}\" to user \"{1}\" at server \"{2}\".", role.Name, user.Username, context.Guild.Name);
                await user.AddRoleAsync(role, new RequestOptions {AuditLogReason = "Застрелился!"});
                var expiry = TimeSpan.FromSeconds(_config.RussianRoulette.WinnerDurationSeconds);
                _backgroundJobClient.Schedule(() => _removeRoleJob.RemoveRole(context.Guild.Id, user.Id, role.Id, "Жив, цел, орёл!"), expiry);
                
            }

            return string.Format(template, userMessage.Author.Mention);
        }
    }

    public static class RandomExtensions
    {
        public static int GetRouletteNumber(this Random rnd, bool isFirstTime, bool isDeveloper, bool isAprilFools)
        {
            if (isDeveloper)
                return 1;

            if (isAprilFools || isFirstTime)
                return 6;

            return rnd.Next(1, 7);
        }
    }
}