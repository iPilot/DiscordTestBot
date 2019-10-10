using StackExchange.Redis;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IRedisDatabaseProvider
    {
        IDatabase Database { get; }
    }
}