using System.Threading.Tasks;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IPastaStore
    {
        Task<string> GetPasta(ulong serverId, string name);
        Task SavePasta(ulong serverId, string name, string content);
    }
}