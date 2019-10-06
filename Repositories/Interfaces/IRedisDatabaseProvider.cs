using System.Threading.Tasks;
using StackExchange.Redis;

namespace DiscordBot.Repositories.Interfaces
{
    public interface IRedisDatabaseProvider
    {
        Task<bool> TryConnect();
        IDatabase Database { get; }
    }
}