using EncrypCoin.API.Data;
using EncrypCoin.API.Models;
using EncrypCoin.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EncrypCoin.API.Repository.Implementations
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Name == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Name == username);
        }

        public async Task<User?> AuthenticateAsync(string email, string passwordHash)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
        }
    }
}
