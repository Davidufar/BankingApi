using BankingApiCore.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BankingApi.Middleware
{
    public class RequireRoleFilter : IAsyncAuthorizationFilter
    {
        private readonly string _requiredRole;
        private readonly IAuthRepository _authRepository;

        // Parameters passed via TypeFilter are injected alongside DI services
        public RequireRoleFilter(string requiredRole, IAuthRepository authRepository)
        {
            _requiredRole = requiredRole;
            _authRepository = authRepository;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity == null || !user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult(); // 401 Unauthorized
                return;
            }

            var clientIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user.FindFirst("ClientId")?.Value;

            if (!int.TryParse(clientIdClaim, out var clientId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Query database/cache for client roles via DI
            var roles = await _authRepository.GetClientRolesAsync(clientId);

            if (!roles.Contains(_requiredRole, StringComparer.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult(); // 403 Forbidden
            }
        }
    }
}
