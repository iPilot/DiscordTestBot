using System.Threading.Tasks;

namespace PochinkiBot.Background.Jobs
{
    public interface IRemoveRoleJob : IBackgroundJob
    {
        Task RemoveRole(ulong guildId, ulong userId, ulong roleId, string reason = null);
    }
}