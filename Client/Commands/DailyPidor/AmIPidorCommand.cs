using System.Threading.Tasks;
using Discord.WebSocket;
using PochinkiBot.Misc;
using PochinkiBot.Repositories.Interfaces;

namespace PochinkiBot.Client.Commands.DailyPidor
{
    [Command("я пидор?")]
    public class AmIPidorCommand : IBotCommand
    {
        private readonly DiscordSocketClient _client;
        private readonly IPidorStore _pidorStore;

        public AmIPidorCommand(DiscordSocketClient client, IPidorStore pidorStore)
        {
            _client = client;
            _pidorStore = pidorStore;
        }

        public async Task Execute(SocketUserMessage userMessage, int argsPos)
        {
            var context = userMessage.GetMessageContext(_client);
            var author = userMessage.Author;
            if (await _pidorStore.AddGuildPidorParticipant(context.Guild.Id, userMessage.Author.Id))
                await userMessage.Channel.SendMessageAsync($"{author.Mention}, я проверю обязательно, как только будет время!");
            else
                await userMessage.Channel.SendMessageAsync($"{author.Mention}, скоро узнаем...");
        }
    }
}