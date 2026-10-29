
using BankingApi.RequestDTOs;
using BankingApiCore.DTOs;
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Mapster;

namespace BankingApiInfrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        this.accountRepository = accountRepository;
    }

    public async Task<int> CreateAsync(int clientId, decimal initialBalance = 0)
    {
        int result = await accountRepository.CreateAsync(clientId, initialBalance);
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        bool result = await accountRepository.DeleteAsync(id);
        return result;
    }

    public async Task<List<AccountDTO>> GetAllWithClientsAsync()
    {
       var list = await accountRepository.GetAllWithClientsAsync();
        List<AccountDTO> result = new List<AccountDTO>();
        foreach (var account in list)
        {
            result.Add(account.Adapt<AccountDTO>());
        }
        return result;
    }

    public async Task<List<AccountDTO>> GetByClientIdAsync(int clientId)
    {
        var list = await accountRepository.GetByClientIdAsync(clientId);


        List<AccountDTO> result = [];
        foreach(var account in list)
        {
            result.Add(account.Adapt<AccountDTO>());
        }
        return result;
    }

    public async Task<AccountDTO?> GetByIdWithClientAsync(int id)
    {
        Account? account = await accountRepository.GetByIdWithClientAsync(id);
        return account.Adapt<AccountDTO>();
    }

    public async Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance)
    {
       return await accountRepository.UpdateBalanceAsync(accountId, newBalance);
    }
    public async Task<bool> MakeTransfer (int userId, int fromAccountId, int toAccountId, decimal amount)
    {
        List<AccountDTO> user_accounts = await GetByClientIdAsync(userId);
        AccountDTO? fromAccount = null;
        AccountDTO? toAccount = null;
        foreach (AccountDTO account in user_accounts)
        {
            if (account.Id == fromAccountId)
            {
                fromAccount = account;
            }
            if (account.Id == toAccountId)
            {
                toAccount = account;
            }
        }
        if (fromAccount != null && toAccount != null)
        {
            return await accountRepository.TransferMoneyAsync(userId, fromAccountId, toAccountId, amount);
        }
        else
        {
            return false;
        }
    }
}
