using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiInfrastructure.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AuthRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<int> RegisterClientAsync(string name, string surname, string email, string phoneNumber, string passwordHash, string ssn, byte status = 1)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new
            {
                Name = name,
                Surname = surname,
                Email = email,
                PhoneNumber = phoneNumber,
                Password = passwordHash,
                SSN = ssn,
                Status = status
            };

            return await connection.ExecuteScalarAsync<int>(
                "dbo.spClients_Register",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ClientAuthRecord?> GetClientByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "SELECT Id, Email, Password FROM dbo.Clients WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<ClientAuthRecord>(sql, new { Email = email });
        }

        public async Task<IEnumerable<string>> GetClientRolesAsync(int clientId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = """
            SELECT r.Name 
            FROM dbo.Roles r 
            JOIN dbo.ClientRoles cr ON r.Id = cr.RoleId 
            WHERE cr.ClientId = @ClientId
            """;
            return await connection.QueryAsync<string>(sql, new { ClientId = clientId });
        }

        public async Task CreateSessionAsync(int clientId, string refreshToken, DateTime expiresAt)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = """
            INSERT INTO dbo.ClientSessions (ClientId, RefreshToken, ExpiresAt)
            VALUES (@ClientId, @RefreshToken, @ExpiresAt)
            """;
            await connection.ExecuteAsync(sql, new { ClientId = clientId, RefreshToken = refreshToken, ExpiresAt = expiresAt });
        }

        public async Task<ClientSessionRecord?> GetSessionByRefreshTokenAsync(string refreshToken)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = """
            SELECT s.Id AS SessionId, s.ClientId, s.LastActivityAt, s.ExpiresAt, s.IsRevoked, c.Email
            FROM dbo.ClientSessions s
            JOIN dbo.Clients c ON s.ClientId = c.Id
            WHERE s.RefreshToken = @RefreshToken AND s.IsRevoked = 0;
            """;
            return await connection.QueryFirstOrDefaultAsync<ClientSessionRecord>(sql, new { RefreshToken = refreshToken });
        }

        public async Task RevokeSessionAsync(Guid sessionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE dbo.ClientSessions SET IsRevoked = 1 WHERE Id = @SessionId";
            await connection.ExecuteAsync(sql, new { SessionId = sessionId });
        }

        public async Task UpdateSessionActivityAsync(Guid sessionId)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE dbo.ClientSessions SET LastActivityAt = GETUTCDATE() WHERE Id = @SessionId";
            await connection.ExecuteAsync(sql, new { SessionId = sessionId });
        }
    }
}
