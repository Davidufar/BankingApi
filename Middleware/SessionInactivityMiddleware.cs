using BankingApiCore.Persistance;
using Dapper;
using System.Security.Claims;

namespace BankingApi.Middleware
{
    public class SessionInactivityMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionInactivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDbConnectionFactory connectionFactory, IConfiguration config)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var clientIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(clientIdClaim, out var clientId))
                {
                    var inactivityLimit = double.Parse(config["JwtSettings:InactivityTimeoutMinutes"]!);
                    using var db = connectionFactory.CreateConnection();

                    
                    const string sql = """
                    SELECT Id, LastActivityAt 
                    FROM dbo.ClientSessions 
                    WHERE ClientId = @ClientId AND IsRevoked = 0 AND ExpiresAt > GETUTCDATE()
                    ORDER BY LastActivityAt DESC;
                    """;

                    var session = await db.QueryFirstOrDefaultAsync<dynamic>(sql, new { ClientId = clientId });

                    if (session != null)
                    {
                        DateTime lastActivity = session.LastActivityAt;
                        if (DateTime.UtcNow - lastActivity > TimeSpan.FromMinutes(inactivityLimit))
                        {
                            
                            await db.ExecuteAsync(
                                "UPDATE dbo.ClientSessions SET IsRevoked = 1 WHERE Id = @Id",
                                new { Id = (Guid)session.Id });

                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Session expired due to inactivity.");
                            return;
                        }

                        
                        await db.ExecuteAsync(
                            "UPDATE dbo.ClientSessions SET LastActivityAt = GETUTCDATE() WHERE Id = @Id",
                            new { Id = (Guid)session.Id });
                    }
                }
            }

            await _next(context);
        }
    }
}
