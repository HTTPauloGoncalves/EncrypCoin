using EncrypCoin.Application.Dtos.Application.Auth.Request;

namespace EncrypCoin.Application.Interfaces.Application
{
    public interface IRefreshTokenService
    {
        Task<(string AccessToken, string RefreshToken)> RefreshAsync(TokenRefreshRequestDto request);
    }
}
