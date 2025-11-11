namespace EncrypCoin.API.Dtos.Application.User.Request
{
    public class UserUpdateRequestDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
