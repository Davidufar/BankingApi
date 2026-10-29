
using BankingApiCore.DTOs;
using BankingApiCore.Models;
using System.Threading.Tasks;

namespace BankingApiCore.Persistance;

public interface IClientService
{
    Task<List<ClientDTO>?> GetAllAsync();
    Task<ClientDTO?> GetByIdAsync(int id);
    Task<ClientDTO?> GetByEmailAsync(string email);
    Task<bool> CreateAsync(Client client);
    Task<bool> UpdateAsync(Client client);
    Task<bool> DeleteAsync(int id);
    Task<List<ClientDTO>> SearchByNameAsync(string? searchTerm);
    Task<List<ClientDTO>> SearchBySSNAsync(string? searchTerm);
}
