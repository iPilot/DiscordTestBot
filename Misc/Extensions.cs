using Discord.Commands;
using Discord.WebSocket;

namespace PochinkiBot.Misc
{
    public static class Extensions
    {
        public static SocketCommandContext GetMessageContext(this SocketUserMessage message, DiscordSocketClient client)
        {
            return new SocketCommandContext(client, message);
        }

        public static string GetUserDisplayName(this SocketGuildUser user)
        {
            return $"{user.Nickname ?? user.Username}{(user.Nickname == null ? "" : $" ({user.Username})")}";
        }
    }
}