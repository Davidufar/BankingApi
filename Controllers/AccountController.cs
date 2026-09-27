using BankingApi.Attributes;
using BankingApi.RequestDTOs;
using BankingApiCore.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BankingApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;

        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }
        [HttpGet]
        [RequireRole("Admin")]
        public async Task<IActionResult> GetAccounts()
        {
            try
            {
                List<AccountDTO> result = await accountService.GetAllWithClientsAsync();
                if (result == null || result.Count == 0)
                {
                    return NotFound("No accounts found.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while retrieving accounts: {ex.Message}");
            }

        }


        [HttpGet("accounts/{id}")]
        [RequireRole("Admin")]
        public async Task<IActionResult> GetByAccountId(int id)
        {
            try
            {
                AccountDTO? result = await accountService.GetByIdWithClientAsync(id);
                if (result == null)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while retrieving account with ID {id}: {ex.Message}");
            }
        }


        [HttpGet("accounts/myaccounts")]
        public async Task<IActionResult> GetMyAccounts()
        {
            try
            {
                int userId = GetCurrentUserId();
                List<AccountDTO> result = await accountService.GetByClientIdAsync(userId);
                if (result == null || result.Count == 0)
                {
                    return NotFound($"No accounts found for client with ID {userId}.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"An error occurred while retrieving accounts for the current user: {ex.Message}");
            }
        }


        [HttpGet("clients/{clientId}")]
        [RequireRole("Admin")]
        public async Task<IActionResult> GetByClientId(int clientId)
        {
            try
            {
                List<AccountDTO> result = await accountService.GetByClientIdAsync(clientId);
                if (result == null || result.Count == 0)
                {
                    return NotFound($"No accounts found for client with ID {clientId}.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while retrieving accounts for client with ID {clientId}: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<ActionResult<bool>> CreateAccount()
        {
            try
            {
                int userId = GetCurrentUserId();
                int result = await accountService.CreateAsync(userId, 0);
                if (result <= 0)
                {
                    return BadRequest("Failed to create account.");
                }
                return Ok($"Account with ID {userId} was successfully created");
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while creating the account: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<bool>> DeleteAccount(int id)
        {
            try
            {
                bool result = await accountService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Account with ID {id} not found.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while deleting the account with ID {id}: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        [RequireRole("Admin")]
        public async Task<ActionResult<bool>> UpdateBalance(int id, [FromBody] decimal newBalance)
        {
            try
            {
                bool result = await accountService.UpdateBalanceAsync(id, newBalance);
                if (!result)
                {
                    return NotFound($"Account with ID {id} not found or balance update failed.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while updating the balance for account with ID {id}: {ex.Message}");
            }
        }

        [HttpPut("clientside/{id}")]
        public async Task<ActionResult<bool>> FulfillBalance(int id, [FromBody] decimal newBalance)
        {
            try
            {
                int userId = GetCurrentUserId();
                List<AccountDTO> user_accounts = await accountService.GetByClientIdAsync(userId);
                foreach (AccountDTO account in user_accounts)
                {
                    if (account.Id == id)
                    {
                        bool result = await accountService.UpdateBalanceAsync(id, newBalance);

                        if (!result)
                        {
                            return NotFound($"Account with ID {id} not found or balance update failed.");
                        }
                        return Ok(result);
                    }
                }
                return BadRequest($"The client was not found with id {userId} ");
            }
            catch(Exception ex)
            {
                throw new ValidationException($"An error occurred while fulfilling the balance for account with ID {id}: {ex.Message}");
            }
        }
        [HttpPut("clientside/transfer/")]
        public async Task<ActionResult<bool>> TransferBetweenAccounts([FromBody] TransferRequest tr)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool result = await accountService.MakeTransfer(userId, tr.fromAccountId, tr.toAccountId, tr.amount);
                if (!result)
                {
                    return BadRequest($"Transfer failed. Please check the account IDs and balance.");
                }
                return Ok(result);
            }
            catch(Exception ex)
            {
                               throw new ValidationException($"An error occurred while transferring between accounts: {ex.Message}");
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User identity claim missing or invalid.");
            }
            return userId;
        }

    }
}
