using EncrypCoin.Application.Exceptions;
using EncrypCoin.Application.Interfaces.Application;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EncrypCoin.API.Middlewares
{
    public class ActiveUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActiveUserMiddleware> _logger;

        public ActiveUserMiddleware(RequestDelegate next, ILogger<ActiveUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            var userPrincipal = context.User;

            // 🔹 Se não estiver autenticado, segue o fluxo normal
            if (userPrincipal?.Identity == null || !userPrincipal.Identity.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            // 🔹 Tenta pegar o ID do usuário do token
            string userIdString = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? userPrincipal.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("Claim de usuário inválida: {Claim}", userIdString);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Invalid authentication claim.",
                    status = 401
                });
                return;
            }

            try
            {
                // 🔹 Valida se o usuário está ativo (regra de negócio no Service)
                var isActive = await userService.IsUserActive(userId);

                if (!isActive)
                {
                    _logger.LogInformation("Usuário inativo tentou acesso (Id: {UserId})", userId);

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Account is deactivated.",
                        status = 403
                    });
                    return;
                }
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuário não encontrado (Id: {UserId})", userId);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = ex.Message,
                    status = 404
                });
                return;
            }

            // 🔹 Se tudo estiver ok, segue o fluxo
            await _next(context);
        }
    }
}