using WordWisp.API.Models.DTOs.Topics;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Topics;

namespace WordWisp.API.Services.Interfaces
{
    public interface ITopicService
    {
        // Для студентов - просмотр публичных топиков
        Task<IEnumerable<TopicListDto>> GetPublicTopicsAsync();
        Task<IEnumerable<TopicListDto>> GetPublicTopicsAsync(int page, int pageSize);
        Task<TopicDto?> GetPublicTopicByIdAsync(int id);
        Task<int> GetPublicTopicsCountAsync();

        // Для преподавателей - работа со своими топиками
        Task<IEnumerable<TopicListDto>> GetTeacherTopicsAsync(int teacherId);
        Task<IEnumerable<TopicListDto>> GetTeacherTopicsAsync(int teacherId, int page, int pageSize);
        Task<TopicDto?> GetTeacherTopicByIdAsync(int id, int teacherId);
        Task<int> GetTeacherTopicsCountAsync(int teacherId);

        // CRUD операции для преподавателей
        Task<TopicDto> CreateTopicAsync(CreateTopicRequest request, int teacherId);
        Task<TopicDto> UpdateTopicAsync(int id, UpdateTopicRequest request, int teacherId);
        Task<bool> DeleteTopicAsync(int id, int teacherId);

        // Проверки доступа
        Task<bool> CanAccessTopicAsync(int topicId, int userId, string userRole);
        Task<bool> CanEditTopicAsync(int topicId, int teacherId);
    }
}
