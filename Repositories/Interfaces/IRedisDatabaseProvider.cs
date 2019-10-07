using System.Threading.Tasks;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IRedisDatabaseProvider
    {
        Task<bool> TryConnect();
        IDatabase Database { get; }
    }
}