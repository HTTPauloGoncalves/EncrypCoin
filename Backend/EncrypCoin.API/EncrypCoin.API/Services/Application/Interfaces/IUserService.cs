using EncrypCoin.API.Dtos.Application.Auth.Response;
using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Dtos.Application.User.Response;

namespace EncrypCoin.API.Services.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterAsync(UserRegisterRequestDto userRegisterRequestDto);
        Task<AuthResponseDto> AuthenticateAsync(UserLoginRequestDto userLoginRequestDto);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<UserResponseDto?> GetByEmailAsync(string email);
        Task<UserResponseDto?> GetByUsernameAsync(string username);
        Task<UserResponseDto?> GetByIdAsync(Guid id);
        Task<List<UserResponseDto>> GetAllAsync();
        Task<UserResponseDto> UpdateUserAdminAsync(UserUpdateAdminRequestDto userUpdateAdminRequestDto);
        Task<UserResponseDto?> UpdateAsync(Guid id, UserUpdateRequestDto dto);
        Task<UserResponseDto> UpdateNameAsync(UserUpdateRequestDto userUpdateRequestDto);
        Task<UserResponseDto> UpdateEmailAsync(UserUpdateRequestDto userUpdateRequestDto);
        Task<UserResponseDto> UpdatePasswordAsync(UserUpdateRequestDto userUpdateRequestDto);
        Task<bool> DeactivateUserAsync(Guid userId);
        Task<bool> DeleteUserAsync(Guid userId);
        Task LogoutAsync(Guid userId);
        Task<bool> SetUserActiveStatusAsync(Guid id, bool status);
    }
}
