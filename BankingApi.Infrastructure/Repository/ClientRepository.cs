
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Dapper;
using System.Data;
using System.Data.Common;

namespace BankingApiInfrastructure.Repository;

    public class ClientRepository : IClientRepository
    {
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> CreateAsync(Client client)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            client.Name,
            client.Surname,
            client.Email,
            client.PhoneNumber,
            client.Password,
            client.SSN,
            Status = (byte)client.Status // Cast enum to TINYINT
        };

        // ExecuteScalarAsync receives the output from SCOPE_IDENTITY()
        var newId = await connection.ExecuteScalarAsync<int>(
            "dbo.spClients_Create",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return newId>0;
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Client>(
            "dbo.spClients_GetById",
            new { Id = id },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<Client?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Client>(
            "dbo.spClients_GetByEmail",
            new { Email = email },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Client>(
            "dbo.spClients_GetAll",
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<bool> UpdateAsync(Client client)
        {
            const string sql = """
            UPDATE Clients
            SET Name = @Name,
                Surname = @Surname,
                Email = @Email,
                PhoneNumber = @PhoneNumber,
                Password = @Password,
                SSN = @SSN
                Status = @Status
            WHERE Id = @Id;
            """;

            using IDbConnection db = _connectionFactory.CreateConnection();
            int rowsAffected = await db.ExecuteAsync(sql, client);
            if(rowsAffected != 0)
                return true;
           
            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"UPDATE Clients SET Status = 4  WHERE Id = @Id";

            using IDbConnection db = _connectionFactory.CreateConnection();
            int rowsAffected = await db.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }
        public async Task<IEnumerable<Client>> SearchByNameAsync(string? searchTerm)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Client>(
                "dbo.spClients_SearchByName",
                new { SearchTerm = searchTerm },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<IEnumerable<Client>> SearchBySSNAsync(string? searchTerm)
        {
            using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Client>(
                "dbo.spClients_SearchBySSN",
                new { SSNTerm = searchTerm },
                commandType: CommandType.StoredProcedure
            );
        }
}
