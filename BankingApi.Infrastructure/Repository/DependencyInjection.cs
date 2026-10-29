using BankingApi.RequestDTOs;
using BankingApiCore.Persistance;
using BankingApiInfrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApiInfrastructure.Repository
{
    public static class ServiceCollectionExtension
    {
        // Accepts IConfiguration passed from Presentation
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
            => services
            .AddScoped<IAccountRepository, AccountRepository>()
            .AddScoped<IClientRepository, ClientRepository>()
            .AddScoped<IDbConnectionFactory, DbConnectionFactory>()
            .AddScoped<IAuthRepository, AuthRepository>()
            .Configure<DatabaseOptions>(configuration.GetSection("ConnectionStrings"));


        public static IServiceCollection AddServices(this IServiceCollection services)
            => services
           .AddScoped<IAccountService, AccountService>()
           .AddScoped<IClientService, ClientService>()
           .AddScoped<TokenService>()
           .AddScoped<IAuthService, AuthService>();
    }      
}
