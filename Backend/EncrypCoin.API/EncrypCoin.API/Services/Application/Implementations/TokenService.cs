using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EncrypCoin.API.Models;
using EncrypCoin.API.Services.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace EncrypCoin.API.Services.Application.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            string? keyConfig = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];
            string? expirationConfig = _configuration["Jwt:ExpirationMinutes"];

            if (string.IsNullOrEmpty(keyConfig))
                throw new InvalidOperationException("Chave JWT não está configurada no appsettings.");

            if (string.IsNullOrEmpty(issuer))
                throw new InvalidOperationException("Emissor JWT não está configurado.");

            if (string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("Audiência JWT não está configurada.");

            if (string.IsNullOrEmpty(expirationConfig))
                throw new InvalidOperationException("Tempo de expiração JWT não está configurado.");

            var key = Encoding.UTF8.GetBytes(keyConfig);
            var expirationMinutes = Convert.ToInt32(expirationConfig);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(ClaimTypes.Role , user.Role),
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            string? keyConfig = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(keyConfig))
                throw new InvalidOperationException("Chave JWT não está configurada no appsettings.");

            var key = Encoding.UTF8.GetBytes(keyConfig);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token inválido.");
            }

            return principal;
        }

        public int GetExpirationMinutes()
        {
            string? expiration = _configuration["Jwt:ExpirationMinutes"];
            if (string.IsNullOrEmpty(expiration))
                throw new InvalidOperationException("Tempo de expiração do JWT não está configurado.");

            return Convert.ToInt32(expiration);
        }

    }
}
