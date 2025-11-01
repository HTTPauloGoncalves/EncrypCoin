namespace EncrypCoin.API.Dtos.Application.User.Response
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
    }
}
