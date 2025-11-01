using System.Net;
using System.Text.Json;
using EncrypCoin.API.Exceptions;

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
            int statusCode;
            string message;

            switch (ex)
            {
                case ValidationException vex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = vex.Message;
                    _logger.LogWarning(vex, "Erro de validação: {Message}", vex.Message);
                    break;

                case AuthenticationException aex:
                    statusCode = (int)HttpStatusCode.Unauthorized;
                    message = aex.Message;
                    _logger.LogWarning(aex, "Erro de autenticação: {Message}", aex.Message);
                    break;

                case NotFoundException nfex:
                    statusCode = (int)HttpStatusCode.NotFound;
                    message = nfex.Message;
                    _logger.LogWarning(nfex, "Recurso não encontrado: {Message}", nfex.Message);
                    break;

                case ArgumentException argex:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    message = argex.Message;
                    _logger.LogWarning(argex, "Argumento inválido: {Message}", argex.Message);
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    message = "Erro interno no servidor.";
                    _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
                    break;
            }

            var result = JsonSerializer.Serialize(new
            {
                status = statusCode,
                message
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(result);
        }
    }
}
