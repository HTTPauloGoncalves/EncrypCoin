using System.Diagnostics;
using System.Net;
using System.Text.Json;
using EncrypCoin.API.Dtos;
using EncrypCoin.API.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EncrypCoin.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            int statusCode;
            string message;
            Dictionary<string, string[]>? errors = null;

            switch (ex)
            {
                case ValidationException vex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = vex.Message;
             
                    if (vex is IHasErrors hasErrors && hasErrors.Errors != null)
                        errors = hasErrors.Errors;
                    _logger.LogWarning(vex, "[{TraceId}] Validation error: {Message}", traceId, vex.Message);
                    break;

                case AuthenticationException aex:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = aex.Message;
                    _logger.LogWarning(aex, "[{TraceId}] Authentication error: {Message}", traceId, aex.Message);
                    break;

                case NotFoundException nfex:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = nfex.Message;
                    _logger.LogWarning(nfex, "[{TraceId}] Not found: {Message}", traceId, nfex.Message);
                    break;

                case ArgumentException argex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = argex.Message;
                    _logger.LogWarning(argex, "[{TraceId}] Invalid argument: {Message}", traceId, argex.Message);
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "Erro interno no servidor.";
                    _logger.LogError(ex, "[{TraceId}] Unhandled error: {Message}", traceId, ex.Message);
                    break;
            }

            var response = new ErrorResponse(traceId, statusCode, message)
            {
                Errors = errors
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var payload = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(payload);
        }
    }

    // Interface helper (optional) to expose structured validation errors from your ValidationException
    public interface IHasErrors
    {
        Dictionary<string, string[]>? Errors { get; }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}
