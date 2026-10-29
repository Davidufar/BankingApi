using BankingApiCore.DTOs;
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApiInfrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepository, TokenService tokenService, IConfiguration config)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            int newClientId = await _authRepository.RegisterClientAsync(
                request.Name, request.Surname, request.Email, request.PhoneNumber, hashedPassword, request.SSN
            );

            var roles = await _authRepository.GetClientRolesAsync(newClientId);

            // Clean call without instantiating Client
            string accessToken = _tokenService.GenerateAccessToken(newClientId, request.Email, roles);
            string refreshToken = _tokenService.GenerateRefreshToken();

            await _authRepository.CreateSessionAsync(newClientId, refreshToken, DateTime.UtcNow.AddDays(7));

            return new AuthResponseDto(newClientId, request.Email, accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var clientRecord = await _authRepository.GetClientByEmailAsync(request.Email);
            if (clientRecord == null || !BCrypt.Net.BCrypt.Verify(request.Password, clientRecord.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var roles = await _authRepository.GetClientRolesAsync(clientRecord.Id);

            // Clean call using record values directly
            string accessToken = _tokenService.GenerateAccessToken(clientRecord.Id, clientRecord.Email, roles);
            string refreshToken = _tokenService.GenerateRefreshToken();

            await _authRepository.CreateSessionAsync(clientRecord.Id, refreshToken, DateTime.UtcNow.AddDays(7));

            return new AuthResponseDto(clientRecord.Id, clientRecord.Email, accessToken, refreshToken);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var session = await _authRepository.GetSessionByRefreshTokenAsync(request.RefreshToken);
            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            double inactivityTimeout = double.Parse(_config["Jwt:InactivityTimeoutMinutes"] ?? "30");
            if (session.LastActivityAt.AddMinutes(inactivityTimeout) < DateTime.UtcNow)
            {
                await _authRepository.RevokeSessionAsync(session.SessionId);
                throw new UnauthorizedAccessException("Session expired due to inactivity.");
            }

            await _authRepository.UpdateSessionActivityAsync(session.SessionId);

            var roles = await _authRepository.GetClientRolesAsync(session.ClientId);

            string newAccessToken = _tokenService.GenerateAccessToken(session.ClientId, session.Email, roles);

            return new AuthResponseDto(session.ClientId, session.Email, newAccessToken, request.RefreshToken);
        }
    }
}
