using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Models.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ApplicationDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int excludeUserId)
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.Id != excludeUserId);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, int excludeUserId)
        {
            return await _context.Users.AnyAsync(u => u.Username == username && u.Id != excludeUserId);
        }

        public async Task<User> UpdateAsync(User user)
        {
            _logger.LogInformation($"Updating user {user.Id}: Username={user.Username}, Name={user.Name}, Surname={user.Surname}, Email={user.Email}, Role={user.Role}");

            // Получаем существующую сущность из контекста
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new ArgumentException("Пользователь не найден");
            }

            _logger.LogInformation($"Found existing user: Username={existingUser.Username}, Name={existingUser.Name}, Surname={existingUser.Surname}, Email={existingUser.Email}, Role={existingUser.Role}");

            // Обновляем свойства
            existingUser.Username = user.Username;
            existingUser.Name = user.Name;
            existingUser.Surname = user.Surname;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsEmailVerified = user.IsEmailVerified;
            existingUser.EmailVerificationCode = user.EmailVerificationCode;
            existingUser.EmailVerificationCodeExpiry = user.EmailVerificationCodeExpiry;

            // Если пароль изменился, обновляем его тоже
            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash != existingUser.PasswordHash)
            {
                existingUser.PasswordHash = user.PasswordHash;
            }

            _logger.LogInformation($"Updated properties: Username={existingUser.Username}, Name={existingUser.Name}, Surname={existingUser.Surname}, Email={existingUser.Email}, Role={existingUser.Role}");

            // Сохраняем изменения
            var changes = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {changes} changes to database");

            return existingUser;
        }
    }
}
