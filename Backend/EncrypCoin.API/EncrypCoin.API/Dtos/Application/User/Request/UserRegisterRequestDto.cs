namespace EncrypCoin.API.Dtos.Application.User.Request
{
    public class UserRegisterRequestDto
    {
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
