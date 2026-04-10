using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Shared
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var response = context.Response;
            response.ContentType = "application/problem+json";

            var (statusCode, title, detail) = exception switch
            {
                DomainException domainEx => (
                    HttpStatusCode.Conflict,
                    "Business rule violation",
                    domainEx.Message
                ),
                UnauthorizedAccessException => (
                    HttpStatusCode.Unauthorized,
                    "Unauthorized",
                    "You are not authorized to perform this action"
                ),
                ArgumentException or ArgumentNullException => (
                    HttpStatusCode.BadRequest,
                    "Invalid request",
                    exception.Message
                ),
                KeyNotFoundException => (
                    HttpStatusCode.NotFound,
                    "Resource not found",
                    exception.Message
                ),
                _ => (
                    HttpStatusCode.InternalServerError,
                    "Server error",
                    "An unexpected error occurred. Please try again later."
                )
            };

            response.StatusCode = (int)statusCode;

            var problemDetails = new
            {
                type = $"https://httpstatuses.io/{response.StatusCode}",
                title = title,
                status = response.StatusCode,
                detail = detail,
                instance = context.Request.Path
            };

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(json);
        }
    }

    public static class GlobalExceptionHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
