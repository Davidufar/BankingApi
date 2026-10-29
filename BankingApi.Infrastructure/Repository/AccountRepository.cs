
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Dapper;
using System.Data;

namespace BankingApiInfrastructure.Repository;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _factory;

    public AccountRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<IEnumerable<Account>> GetAllWithClientsAsync()
    {
        const string sql = """
        SELECT a.Id, a.ClientId, a.AccountNumber, a.Balance, a.CreatedAt,
               c.Id, c.Name, c.Surname, c.Email, c.PhoneNumber, c.Password, c.SSN, c.Status, c.CreatedAt
        FROM Accounts a
        INNER JOIN Clients c ON a.ClientId = c.Id
        ORDER BY a.Id DESC
        """;

        using IDbConnection db = _factory.CreateConnection();

        return await db.QueryAsync<Account, Client, Account>(
            sql,
            (account, client) =>
            {
                account.Client = client;
                return account;
            },
            splitOn: "Id"
        );
    }

    public async Task<Account?> GetByIdWithClientAsync(int id)
    {
        const string sql = """
        SELECT a.Id, a.ClientId, a.AccountNumber, a.Balance, a.CreatedAt,
               c.Id, c.Name, c.Surname, c.Email, c.PhoneNumber, c.Password, c.SSN, c.Status, c.CreatedAt
        FROM Accounts a
        INNER JOIN Clients c ON a.ClientId = c.Id
        WHERE a.Id = @Id
        """;

        using IDbConnection db = _factory.CreateConnection();

        var result = await db.QueryAsync<Account, Client, Account>(
            sql,
            (account, client) =>
            {
                account.Client = client;
                return account;
            },
            new { Id = id },
            splitOn: "Id"
        );

        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<Account>> GetByClientIdAsync(int clientId)
    {
        const string sql = """
        SELECT Id, ClientId, AccountNumber, Balance, CreatedAt
        FROM Accounts
        WHERE ClientId = @ClientId
        ORDER BY Id DESC
        """;

        using IDbConnection db = _factory.CreateConnection();
        return await db.QueryAsync<Account>(sql, new { ClientId = clientId });
    }

    public async Task<int> CreateAsync(int clientId, decimal initialBalance = 0)
    {
        string accountNumber = Generate15DigitAccountNumber();

        const string sql = """
        INSERT INTO Accounts (ClientId, AccountNumber, Balance, CreatedAt)
        VALUES (@ClientId, @AccountNumber, @Balance, GETUTCDATE());
        
        SELECT CAST(SCOPE_IDENTITY() as int);
        """;

        using IDbConnection db = _factory.CreateConnection();
        return await db.ExecuteScalarAsync<int>(sql, new
        {
            ClientId = clientId,
            AccountNumber = accountNumber,
            Balance = initialBalance
        });
    }

    public async Task<bool> UpdateBalanceAsync(int accountId, decimal newBalance)
    {
        const string sql = """
        UPDATE Accounts
        SET Balance = @Balance
        WHERE Id = @Id
        """;

        using IDbConnection db = _factory.CreateConnection();
        int rows = await db.ExecuteAsync(sql, new { Id = accountId, Balance = newBalance });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Accounts WHERE Id = @Id";

        using IDbConnection db = _factory.CreateConnection();
        int rows = await db.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }


    public async Task<bool> TransferMoneyAsync(int clientId, int fromAccountId, int toAccountId, decimal amount)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        // Start a database transaction
        using var dbTransaction = connection.BeginTransaction();

        try
        {
            // 1. Deduct from source account (validates ownership & balance in one atomic update)
            const string deductSql = """
                UPDATE dbo.Accounts
                SET Balance = Balance - @Amount
                WHERE Id = @FromAccountId 
                  AND ClientId = @ClientId 
                  AND Balance >= @Amount;
                """;

            int rowsDeducted = await connection.ExecuteAsync(
                deductSql,
                new { FromAccountId = fromAccountId, ClientId = clientId, Amount = amount },
                dbTransaction
            );

            if (rowsDeducted == 0)
            {
                // Account not found, not owned by user, or insufficient funds
                dbTransaction.Rollback();
                return false;
            }

            // 2. Add to target account (validates target account ownership)
            const string addSql = """
                UPDATE dbo.Accounts
                SET Balance = Balance + @Amount
                WHERE Id = @ToAccountId 
                  AND ClientId = @ClientId;
                """;

            int rowsAdded = await connection.ExecuteAsync(
                addSql,
                new { ToAccountId = toAccountId, ClientId = clientId, Amount = amount },
                dbTransaction
            );

            if (rowsAdded == 0)
            {
                // Target account not found or not owned by user
                dbTransaction.Rollback();
                return false;
            }

            // 3. Insert record into dbo.Transactions
            const string insertTransactionSql = """
                INSERT INTO dbo.Transactions (FromAccountId, ToAccountId, Amount, CreatedAt)
                VALUES (@FromAccountId, @ToAccountId, @Amount, GETUTCDATE());
                """;

            await connection.ExecuteAsync(
                insertTransactionSql,
                new { FromAccountId = fromAccountId, ToAccountId = toAccountId, Amount = amount },
                dbTransaction
            );

            // 4. Commit all changes simultaneously
            dbTransaction.Commit();
            return true;
        }
        catch
        {
            // Undo all operations if an exception occurs
            dbTransaction.Rollback();
            throw;
        }
    }
    private static string Generate15DigitAccountNumber()
    {
        var random = Random.Shared;
        char[] digits = new char[15];

        digits[0] = (char)('1' + random.Next(0, 9));

        for (int i = 1; i < 15; i++)
        {
            digits[i] = (char)('0' + random.Next(0, 10));
        }

        return new string(digits);
    }
}
