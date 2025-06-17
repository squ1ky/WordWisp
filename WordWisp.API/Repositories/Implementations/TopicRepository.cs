using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class TopicRepository : ITopicRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TopicRepository> _logger;

        public TopicRepository(ApplicationDbContext context, ILogger<TopicRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Общие методы
        public async Task<Topic?> GetByIdAsync(int id)
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Topic?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .Include(t => t.Materials)
                .Include(t => t.Exercises)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Topics.AnyAsync(t => t.Id == id);
        }

        // Для студентов - только публичные топики
        public async Task<IEnumerable<Topic>> GetPublicTopicsAsync()
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Topic>> GetPublicTopicsAsync(int skip, int take)
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetPublicTopicsCountAsync()
        {
            return await _context.Topics.CountAsync(t => t.IsPublic);
        }

        // Для преподавателей - свои топики
        public async Task<IEnumerable<Topic>> GetTopicsByTeacherAsync(int teacherId)
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedBy == teacherId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Topic>> GetTopicsByTeacherAsync(int teacherId, int skip, int take)
        {
            return await _context.Topics
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedBy == teacherId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTopicsCountByTeacherAsync(int teacherId)
        {
            return await _context.Topics.CountAsync(t => t.CreatedBy == teacherId);
        }

        public async Task<int> GetTotalMaterialsCountByTeacherAsync(int teacherId)
        {
            return await _context.Materials
                .CountAsync(m => m.Topic.CreatedBy == teacherId);
        }

        // CRUD операции для преподавателей
        public async Task<Topic> CreateAsync(Topic topic)
        {
            _logger.LogInformation($"Creating topic: {topic.Title} by user {topic.CreatedBy}");
            
            topic.CreatedAt = DateTime.UtcNow;
            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();
            
            // Загружаем созданный топик с пользователем
            return await GetByIdAsync(topic.Id) ?? topic;
        }

        public async Task<Topic> UpdateAsync(Topic topic)
        {
            _logger.LogInformation($"Updating topic {topic.Id}: {topic.Title}");
            
            var existingTopic = await _context.Topics.FindAsync(topic.Id);
            if (existingTopic == null)
            {
                throw new ArgumentException("Топик не найден");
            }

            // Обновляем поля
            existingTopic.Title = topic.Title;
            existingTopic.Description = topic.Description;
            existingTopic.IsPublic = topic.IsPublic;

            await _context.SaveChangesAsync();
            
            // Возвращаем обновленный топик с деталями
            return await GetByIdAsync(existingTopic.Id) ?? existingTopic;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation($"Deleting topic {id}");
            
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null)
            {
                return false;
            }

            _context.Topics.Remove(topic);
            var changes = await _context.SaveChangesAsync();
            
            return changes > 0;
        }

        // Проверки доступа
        public async Task<bool> IsTopicOwnedByTeacherAsync(int topicId, int teacherId)
        {
            return await _context.Topics
                .AnyAsync(t => t.Id == topicId && t.CreatedBy == teacherId);
        }

        public async Task<bool> IsTopicPublicAsync(int topicId)
        {
            return await _context.Topics
                .Where(t => t.Id == topicId)
                .Select(t => t.IsPublic)
                .FirstOrDefaultAsync();
        }

        // Подсчет связанных данных
        public async Task<int> GetMaterialsCountAsync(int topicId)
        {
            return await _context.Materials
                .CountAsync(m => m.TopicId == topicId);
        }

        public async Task<int> GetExercisesCountAsync(int topicId)
        {
            return await _context.Exercises
                .Where(e => e.Material.TopicId == topicId)
                .CountAsync();
        }
    }
}
