using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace BankingApiInfrastructure.Repository
{
    public class CachedAuthRepository : IAuthRepository
    {
        private readonly IAuthRepository _innerRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedAuthRepository> _logger;

        public CachedAuthRepository(
            IAuthRepository innerRepository,
            IMemoryCache cache,
            ILogger<CachedAuthRepository> logger)
        {
            _innerRepository = innerRepository;
            _cache = cache;
            _logger = logger;
        }

        // --- CACHED METHODS ---

        public async Task CreateSessionAsync(int clientId, string refreshToken, DateTime expiresAt)
        {
            // 1. Save to Database
            await _innerRepository.CreateSessionAsync(clientId, refreshToken, expiresAt);

            // 2. Store directly in MemoryCache
            try
            {
                var session = new ClientSessionRecord
                {
                    ClientId = clientId,
                    ExpiresAt = expiresAt,
                    IsRevoked = false,
                    LastActivityAt = DateTime.UtcNow
                };

                var options = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(expiresAt);

                _cache.Set(GetCacheKey(refreshToken), session, options);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache refresh token in MemoryCache.");
            }
        }

        public async Task<ClientSessionRecord?> GetSessionByRefreshTokenAsync(string refreshToken)
        {
            var cacheKey = GetCacheKey(refreshToken);

            // 1. Try reading directly from MemoryCache
            if (_cache.TryGetValue(cacheKey, out ClientSessionRecord? cachedSession) && cachedSession != null)
            {
                return cachedSession;
            }

            // 2. Fall back to Database
            try
            {
                var dbSession = await _innerRepository.GetSessionByRefreshTokenAsync(refreshToken);

                // Re-populate MemoryCache if active session found in DB
                if (dbSession != null && !dbSession.IsRevoked)
                {
                    var options = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(dbSession.ExpiresAt);

                    _cache.Set(cacheKey, dbSession, options);
                }

                return dbSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database lookup failed for refresh token.");
                throw;
            }
        }

        public async Task RevokeSessionAsync(Guid sessionId)
        {
            await _innerRepository.RevokeSessionAsync(sessionId);
        }

        public async Task UpdateSessionActivityAsync(Guid sessionId)
        {
            await _innerRepository.UpdateSessionActivityAsync(sessionId);
        }

        // --- DIRECT DELEGATION TO INNER REPOSITORY ---

        public Task<int> RegisterClientAsync(string name, string surname, string email, string phoneNumber, string passwordHash, string ssn, byte status = 1)
            => _innerRepository.RegisterClientAsync(name, surname, email, phoneNumber, passwordHash, ssn, status);

        public Task<ClientAuthRecord?> GetClientByEmailAsync(string email)
            => _innerRepository.GetClientByEmailAsync(email);

        public Task<IEnumerable<string>> GetClientRolesAsync(int clientId)
            => _innerRepository.GetClientRolesAsync(clientId);

        // --- HELPER ---

        private static string GetCacheKey(string refreshToken) => $"refreshToken:{refreshToken}";
    }
}
