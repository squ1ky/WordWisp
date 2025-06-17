using WordWisp.API.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface ITopicRepository
    {
        // Общие методы
        Task<Topic?> GetByIdAsync(int id);
        Task<Topic?> GetByIdWithDetailsAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Для студентов - только публичные топики
        Task<IEnumerable<Topic>> GetPublicTopicsAsync();
        Task<IEnumerable<Topic>> GetPublicTopicsAsync(int skip, int take);
        Task<int> GetPublicTopicsCountAsync();
        
        // Для преподавателей - свои топики
        Task<IEnumerable<Topic>> GetTopicsByTeacherAsync(int teacherId);
        Task<IEnumerable<Topic>> GetTopicsByTeacherAsync(int teacherId, int skip, int take);
        Task<int> GetTopicsCountByTeacherAsync(int teacherId);
        
        // CRUD операции для преподавателей
        Task<Topic> CreateAsync(Topic topic);
        Task<Topic> UpdateAsync(Topic topic);
        Task<bool> DeleteAsync(int id);
        
        // Проверки доступа
        Task<bool> IsTopicOwnedByTeacherAsync(int topicId, int teacherId);
        Task<bool> IsTopicPublicAsync(int topicId);
        
        // Подсчет связанных данных
        Task<int> GetMaterialsCountAsync(int topicId);
        Task<int> GetExercisesCountAsync(int topicId);
    }
}
