namespace EncrypCoin.API.Dtos.Application.User.Response
{
    public class UserLoginResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
