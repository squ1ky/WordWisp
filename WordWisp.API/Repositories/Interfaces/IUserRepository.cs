using WordWisp.API.Models.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User> UpdateAsync(User user);

        // Новые методы
        Task<User?> GetByIdAsync(int id);
        Task<bool> ExistsByEmailAsync(string email, int excludeUserId);
        Task<bool> ExistsByUsernameAsync(string username, int excludeUserId);
    }
}
