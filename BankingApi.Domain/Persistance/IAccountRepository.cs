
using BankingApiCore.Models;

namespace BankingApiCore.Persistance;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllWithClientsAsync();
    Task<Account?> GetByIdWithClientAsync(int id);
    Task<IEnumerable<Account>> GetByClientIdAsync(int clientId);
    Task<int> CreateAsync(int clientId, decimal initialBalance = 0);
    Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance);
    Task<bool> DeleteAsync(int id);
    Task<bool> TransferMoneyAsync(int clientId, int fromAccountId, int toAccountId, decimal amount);
}
