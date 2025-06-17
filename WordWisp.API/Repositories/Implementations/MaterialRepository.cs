using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Models.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MaterialRepository> _logger;

        public MaterialRepository(ApplicationDbContext context, ILogger<MaterialRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Material> CreateAsync(Material material)
        {
            material.CreatedAt = DateTime.UtcNow;
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<Material?> GetByIdAsync(int id)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Material?> GetByIdWithTopicAsync(int id)
        {
            return await _context.Materials
                .Include(m => m.Topic)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Material>> GetByTopicIdAsync(int topicId)
        {
            return await _context.Materials
                .Where(m => m.TopicId == topicId)
                .OrderBy(m => m.Order)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Material>> GetPublicByTopicIdAsync(int topicId)
        {
            return await _context.Materials
                .Where(m => m.TopicId == topicId && m.IsPublic)
                .OrderBy(m => m.Order)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Material> UpdateAsync(Material material)
        {
            var existingMaterial = await _context.Materials.FindAsync(material.Id);
            if (existingMaterial == null)
            {
                throw new ArgumentException("Материал не найден");
            }

            // Обновляем свойства
            existingMaterial.Title = material.Title;
            existingMaterial.Description = material.Description;
            existingMaterial.Content = material.Content;
            existingMaterial.ExternalUrl = material.ExternalUrl;
            existingMaterial.IsPublic = material.IsPublic;
            existingMaterial.Order = material.Order;
            existingMaterial.UpdatedAt = DateTime.UtcNow;

            // Обновляем файловые поля только если они изменились
            if (!string.IsNullOrEmpty(material.FilePath))
            {
                existingMaterial.FilePath = material.FilePath;
                existingMaterial.FileSize = material.FileSize;
                existingMaterial.MimeType = material.MimeType;
                existingMaterial.OriginalFileName = material.OriginalFileName;
            }

            await _context.SaveChangesAsync();
            return existingMaterial;
        }

        public async Task DeleteAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material != null)
            {
                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Materials.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> BelongsToTopicAsync(int materialId, int topicId)
        {
            return await _context.Materials
                .AnyAsync(m => m.Id == materialId && m.TopicId == topicId);
        }

        public async Task<int> GetMaxOrderByTopicIdAsync(int topicId)
        {
            var maxOrder = await _context.Materials
                .Where(m => m.TopicId == topicId)
                .MaxAsync(m => (int?)m.Order);
            
            return maxOrder ?? 0;
        }

        public async Task<List<Material>> GetByTopicIdOrderedAsync(int topicId)
        {
            return await _context.Materials
                .Include(m => m.Topic)
                .Where(m => m.TopicId == topicId)
                .OrderBy(m => m.Order)
                .ThenBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
