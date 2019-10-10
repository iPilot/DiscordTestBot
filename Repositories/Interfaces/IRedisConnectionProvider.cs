using System.Threading.Tasks;
using StackExchange.Redis;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IRedisConnectionProvider
    {
        ConnectionMultiplexer Connection { get; }
        Task<bool> TryConnect();
    }
}