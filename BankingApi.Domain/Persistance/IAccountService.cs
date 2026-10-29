
using BankingApiCore.DTOs;

namespace BankingApi.RequestDTOs;

    public interface IAccountService
    {
        Task<List<AccountDTO>> GetAllWithClientsAsync();
        Task<AccountDTO?> GetByIdWithClientAsync(int id);
        Task<List<AccountDTO>> GetByClientIdAsync(int clientId);
        Task<int> CreateAsync(int clientId, decimal initialBalance = 0);
        Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance);
        Task<bool> DeleteAsync(int id);
        Task<bool> MakeTransfer(int userId, int fromAccountId, int toAccountId, decimal amount);
    }

