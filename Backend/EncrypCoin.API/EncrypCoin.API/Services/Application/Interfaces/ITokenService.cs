using System.Security.Claims;
using EncrypCoin.API.Models;

namespace EncrypCoin.API.Services.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        int GetExpirationMinutes();
    }
}
