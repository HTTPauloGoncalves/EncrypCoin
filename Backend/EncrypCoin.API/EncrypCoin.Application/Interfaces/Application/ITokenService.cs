using System.Security.Claims;
using EncrypCoin.Domain.Entities;

namespace EncrypCoin.Application.Interfaces.Application
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        int GetExpirationMinutes();
    }
}
