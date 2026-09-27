using BankingApi.Attributes;
using BankingApi.Exceptions;
using BankingApiCore.DTOs;
using BankingApiCore.Persistance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [SkipLogging]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                if (result == null)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
             throw new ValidationException($"An error occurred during registration: {ex.Message}");
            }
        }

        [HttpPost("login")]
        [SkipLogging]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                if (result == null)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                               throw new ValidationException($"An error occurred during login: {ex.Message}");
            }
        }

        [HttpPost("refresh")]
        [SkipLogging]
        public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(request);
                if (result == null)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred during token refresh: {ex.Message}");
            }   
        }
    }
}
