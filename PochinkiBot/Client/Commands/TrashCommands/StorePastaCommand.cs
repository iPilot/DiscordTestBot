//using System.Threading.Tasks;
//using Discord.WebSocket;
//using Hangfire.AspNetCore;
//using PochinkiBot.Misc;
//using PochinkiBot.Repositories.Interfaces;

//namespace PochinkiBot.Client.Commands.TrashCommands
//{
//    [Command("сохрани:")]
//    [CommandArg("Name", typeof(string), 0)]
//    [CommandArg("Content", typeof(string), 1)]
//    public class StorePastaCommand : IBotCommand
//    {
//        private readonly DiscordSocketClient _client;
//        private readonly ICommandArgsParser _parser;
//        private readonly IPastaStore _pastaStore;

//        public StorePastaCommand(DiscordSocketClient client, ICommandArgsParser parser, IPastaStore pastaStore)
//        {
//            _client = client;
//            _parser = parser;
//            _pastaStore = pastaStore;
//        }

//        public async Task Execute(SocketUserMessage userMessage, int argsPos)
//        {
//            if (userMessage.Author.IsBot || userMessage.Author.IsWebhook)
//                return;

//            var context = userMessage.GetMessageContext(_client);
//            if (!context.Guild.GetUser(context.User.Id).GuildPermissions.Administrator)
//            {
//                await userMessage.DeleteAsync();
//                return;
//            }

//            var args = _parser.ParseArgs<PastaArgsModel>(this, userMessage, argsPos);
//            if (args == null)
//            {
//                await userMessage.DeleteAsync();
//                return;
//            }

//            await _pastaStore.SavePasta(context.Guild.Id, args.Name, args.Content);
//            await userMessage.Channel.SendMessageAsync("Сохранил.");
//        }
//    }

//    public class PastaArgsModel
//    {
//        public string Name { get; set; }
//        public string Content { get; set; }
//    }
//}