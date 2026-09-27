using BankingApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BankingApi.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception processing {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var problemDetails = exception switch
            {
                NotFoundException notFoundEx => new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource Not Found",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                },
                ValidationException validationEx => new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = validationEx.Message,
                    Instance = context.Request.Path,
                    Extensions = { ["errors"] = validationEx.Errors }
                },
                CustomUnauthorizedAccessException customUnauthorizedAccessException =>new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authorization Error",
                    Detail = customUnauthorizedAccessException.Message,
                    Instance = context.Request.Path,
                },
                _ => new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = _env.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later.",
                    Instance = context.Request.Path
                }
            };

            context.Response.StatusCode = problemDetails.Status.Value;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
        }
    }
}
