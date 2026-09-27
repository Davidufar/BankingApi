using BankingApi.Attributes;
using BankingApi.CreationModels;
using BankingApi.Exceptions;
using BankingApiCore.DTOs;
using BankingApiCore.Models;
using BankingApiCore.Persistance;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApi.Controllers
{
    [ApiController]
    [Route("api/clients")]
    [Authorize] // Requires valid JWT token for all endpoints by default
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ILogger<ClientController> _logger;

        public ClientController(IClientService clientService, ILogger<ClientController> _logger)
        {
            this._clientService = clientService;
            this._logger = _logger;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ClientDTO>> GetMe()
        {
            int currentUserId = GetCurrentUserId();
            try
            {
                ClientDTO? client = await _clientService.GetByIdAsync(currentUserId);
                if (client == null)
                {
                    _logger.LogInformation("Couldnt find client");
                    return NotFound();
                }
                return Ok(client);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Validation error occured when getting client by clientId = {currentUserId}");
            }
        }

        [HttpGet]
        [RequireRole("Admin")]
        public async Task<ActionResult<List<ClientDTO>>> GetAll()
        {
            try
            {
                List<ClientDTO>? clients = await _clientService.GetAllAsync();
                if (clients == null || clients.Count == 0)
                {
                    return NotFound();
                }
                return Ok(clients);
            }
            catch(Exception ex)
            {
                throw new ValidationException("Validation error occured when getting all clients");
            }
        }


        [HttpGet("searchbyname/{name}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<List<ClientDTO>>> SearchByName(string? name)
        {
            try
            {
                var clients = await _clientService.SearchByNameAsync(name);
                if (clients == null || clients.Count == 0)
                {
                    return NotFound();
                }
                return Ok(clients);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"Validation error occured when searching clients by name = {name}");
            }
        }


        [HttpGet("searchbyssn/{ssn}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<List<ClientDTO>>> SearchBySSN(string? ssn)
        {
            try
            {
                var clients = await _clientService.SearchBySSNAsync(ssn);
                if (clients == null || clients.Count == 0)
                {
                    return NotFound();
                }
                return Ok(clients);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"Validation error occured when searching clients by ssn = {ssn}");
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDTO>> GetById(int id)
        {
            int currentUserId = GetCurrentUserId();
            try
            {
                if (id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                ClientDTO? client = await _clientService.GetByIdAsync(id);
                if (client == null)
                {
                    return NotFound();
                }
                return Ok(client);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"Validation error occured when getting client by clientId = {id}");
            }
        }

        [HttpPost]
        [RequireRole("Admin")]
        public async Task<ActionResult<bool>> CreateAsync([FromBody] ClientRequest createClientDTO)
        {
            try {
                Client client = createClientDTO.Adapt<Client>();
                client.Status = BankingApiCore.Enums.ClientStatus.Active;
                bool result = await _clientService.CreateAsync(client);
                if (!result)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                               throw new ValidationException("Validation error occured when creating client");
            }

        }


        [HttpPut("{id}")]
        public async Task<ActionResult<bool>> UpdateAsync(int id, [FromBody] ClientRequest updateClientDTO)
        {
            int currentUserId = GetCurrentUserId();
            try
            {
                if (id != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                Client client = updateClientDTO.Adapt<Client>();
                client.Id = id;
                bool result = await _clientService.UpdateAsync(client);
                if (!result)
                {
                    return NotFound(result);
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"Validation error occured when updating client with clientId = {id}");
            }
        }


        [HttpDelete("{id}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<bool>> DeleteAsync(int id)
        {
            bool result = await _clientService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new CustomUnauthorizedAccessException("User identity claim missing or invalid.");
            }
            return userId;
        }
    }
}
