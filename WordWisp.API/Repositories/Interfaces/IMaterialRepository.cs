using WordWisp.API.Models.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IMaterialRepository
    {
        Task<Material> CreateAsync(Material material);
        Task<Material?> GetByIdAsync(int id);
        Task<Material?> GetByIdWithTopicAsync(int id);
        Task<List<Material>> GetByTopicIdAsync(int topicId);
        Task<List<Material>> GetPublicByTopicIdAsync(int topicId);
        Task<Material> UpdateAsync(Material material);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> BelongsToTopicAsync(int materialId, int topicId);
        Task<int> GetMaxOrderByTopicIdAsync(int topicId);
        Task<List<Material>> GetByTopicIdOrderedAsync(int topicId);
    }
}
