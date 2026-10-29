

using BankingApiCore.Models;

namespace BankingApiCore.Persistance;

    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetAllAsync();
        Task<Client?> GetByIdAsync(int id);
        Task<Client?> GetByEmailAsync(string email);
        Task<bool> CreateAsync(Client client);
        Task<bool> UpdateAsync(Client client);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Client>> SearchByNameAsync(string? searchTerm);
        Task<IEnumerable<Client>> SearchBySSNAsync(string? searchTerm);
}
