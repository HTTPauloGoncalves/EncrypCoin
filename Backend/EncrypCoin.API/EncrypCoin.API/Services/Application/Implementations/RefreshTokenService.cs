using EncrypCoin.API.Dtos.Application.Auth.Request;
using EncrypCoin.API.Repository.Interfaces;
using EncrypCoin.API.Services.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace EncrypCoin.API.Services.Application.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public RefreshTokenService(ITokenService tokenService, IUserRepository userRepository)
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        public async Task<(string AccessToken, string RefreshToken)> RefreshAsync(TokenRefreshRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("AccessToken e RefreshToken são obrigatórios.");

            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

            if (email == null)
                throw new UnauthorizedAccessException("Token inválido.");

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userRepository.UpdateAsync(user);

            return (newAccessToken, newRefreshToken);
        }
    }
}
