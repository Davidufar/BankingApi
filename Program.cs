


using BankingApi.Middleware;
using BankingApiCore.Persistance;
using BankingApiInfrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json;

namespace BankingApi
{

    public static class Program
    {
        public static void Main(string[] args)
        {
                        Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);


                var connectionString = builder.Configuration.GetConnectionString("MyLocalDatabase")
    ?? builder.Configuration["DatabaseOptions:ConnectionString"];

                // Register Health Checks
                builder.Services.AddHealthChecks()
                    .AddSqlServer(
                        connectionString: connectionString!,
                        healthQuery: "SELECT 1;",
                        name: "sql_server",
                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                        tags: new[] { "db", "sql", "infrastructure" });

                // Add services to the container.
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services));

                builder.Services.AddMemoryCache();
                ServiceCollectionExtension.AddRepositories(builder.Services, builder.Configuration);
                ServiceCollectionExtension.AddServices(builder.Services);
                builder.Services.AddScoped<AuthRepository>();
                builder.Services.AddScoped<IAuthRepository>(provider =>
                new CachedAuthRepository(
                    provider.GetRequiredService<AuthRepository>(),
                    provider.GetRequiredService<IMemoryCache>(),
                    provider.GetRequiredService<ILogger<CachedAuthRepository>>()
                ));
                builder.Services.AddServices();
                builder.Services.AddControllers();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
                    ClockSkew = TimeSpan.Zero // Strict expiration without 5-minute delay
                };
            });
                builder.Services.AddAuthorization();

                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Banking API", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Input your JWT token directly: Bearer {your_token}"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                });

                var app = builder.Build();

                app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
                app.UseMiddleware<RequestResponseLoggingMiddleware>();
                app.UseSerilogRequestLogging();
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();


                app.MapControllers();

                app.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            status = report.Status.ToString(),
                            totalDurationMs = report.TotalDuration.TotalMilliseconds,
                            entries = report.Entries.Select(e => new
                            {
                                component = e.Key,
                                status = e.Value.Status.ToString(),
                                description = e.Value.Description,
                                durationMs = e.Value.Duration.TotalMilliseconds,
                                exception = e.Value.Exception?.Message
                            })
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
                        {
                            WriteIndented = true
                        }));
                    }
                });
                app.Run();
                
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Banking API terminated unexpectedly on startup!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

      
    }
}
