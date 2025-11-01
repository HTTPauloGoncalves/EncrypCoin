namespace EncrypCoin.API.Dtos.Application.Auth.Request
{
    public class TokenRefreshRequestDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
