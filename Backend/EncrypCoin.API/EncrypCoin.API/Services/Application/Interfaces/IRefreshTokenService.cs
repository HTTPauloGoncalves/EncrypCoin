using EncrypCoin.API.Dtos.Application.Auth.Request;

namespace EncrypCoin.API.Services.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<(string AccessToken, string RefreshToken)> RefreshAsync(TokenRefreshRequestDto request);
    }
}
