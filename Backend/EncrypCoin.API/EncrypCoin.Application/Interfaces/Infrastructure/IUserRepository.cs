using EncrypCoin.Domain.Entities;

namespace EncrypCoin.Application.Interfaces.Infrastructure
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User?> AuthenticateAsync(string email, string passwordHash);
    }
}
