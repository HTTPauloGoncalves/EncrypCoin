using EncrypCoin.API.Repository.Interfaces;
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

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
            var userPrincipal = context.User;
            if (userPrincipal?.Identity == null || !userPrincipal.Identity.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            string userIdString = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? userPrincipal.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdString, out var userId))
            {
                // Se o token não trazer um GUID válido, bloqueia por segurança.
                _logger.LogWarning("Request com claim de usuário inválida: {Claim}", userIdString);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid authentication claim.");
                return;
            }

            // 3) Consulta o repositório para verificar se o usuário está ativo
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Usuário não encontrado (Id: {UserId}) ao validar status.", userId);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("User not found.");
                return;
            }

            if (!user.IsActive)
            {
                _logger.LogInformation("Acesso negado: usuário inativo (Id: {UserId})", userId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Account is deactivated. Contact support.");
                return;
            }

            await _next(context);
        }
    }
}
