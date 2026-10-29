using BankingApiCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiCore.Persistance
{
    public interface IAuthRepository
    {
        Task<int> RegisterClientAsync(string name, string surname, string email, string phoneNumber, string passwordHash, string ssn, byte status = 1);
        Task<ClientAuthRecord?> GetClientByEmailAsync(string email);
        Task<IEnumerable<string>> GetClientRolesAsync(int clientId);
        Task CreateSessionAsync(int clientId, string refreshToken, DateTime expiresAt);
        Task<ClientSessionRecord?> GetSessionByRefreshTokenAsync(string refreshToken);
        Task RevokeSessionAsync(Guid sessionId);
        Task UpdateSessionActivityAsync(Guid sessionId);
    }
}
