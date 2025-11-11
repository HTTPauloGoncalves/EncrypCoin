using EncrypCoin.API.Enums.User;

namespace EncrypCoin.API.Dtos.Application.User.Request
{
    public class UserUpdateAdminRequestDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public UserRole? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
