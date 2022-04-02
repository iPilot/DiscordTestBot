using System;
using System.Threading.Tasks;

namespace PochinkiBot.Repositories.Interfaces
{
    public interface IBirthDayStore
    {
        Task SaveBirthDate(ulong userId, DateTime birthDate);
        Task<DateTime?> GetBirthDate(ulong userId);
    }
}