using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Serilog;

namespace PochinkiBot.Background.Jobs
{
    public class RemoveRoleJob : IRemoveRoleJob
    {
        private readonly DiscordSocketClient _client;
        private static readonly ILogger Logger = Log.Logger;

        public RemoveRoleJob(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task RemoveRole(ulong guildId, ulong userId, ulong roleId, string reason = null)
        {
            if (_client.ConnectionState != ConnectionState.Connected)
                return;

            var guild = _client.Guilds.FirstOrDefault(g => g.Id == guildId);

            var user = guild?.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return;

            var role = guild.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null)
                return;

            var options = reason == null ? null : new RequestOptions { AuditLogReason = reason };
            await user.RemoveRoleAsync(role, options);

            Logger.Information("Removed role \"{0}\" of user \"{1}\" at server \"{2}\".", role.Name, user.Username, guild.Name);
        }
    }
}