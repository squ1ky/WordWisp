using WordWisp.API.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IDictionaryRepository
    {
        Task<List<Dictionary>> GetByUserIdAsync(int userId);
        Task<List<Dictionary>> GetUserPublicDictionariesAsync(int userId);
        Task<Dictionary?> GetByIdAndUserIdAsync(int id, int userId);
        Task<Dictionary?> GetPublicByIdAsync(int id);
        Task<Dictionary> CreateAsync(Dictionary dictionary);
        Task<Dictionary> UpdateAsync(Dictionary dictionary);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id, int userId);
        Task<bool> ToggleVisibilityAsync(int id, int userId);
        Task<Dictionary?> GetByIdAndUserIdForUpdateAsync(int id, int userId);
        Task<int> GetDictionariesCountByUserIdAsync(int userId);

    }
}

