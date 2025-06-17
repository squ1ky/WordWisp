using WordWisp.API.Constants;
using WordWisp.API.Entities;
using WordWisp.API.Models.DTOs.Topics;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Topics;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;
        private readonly ILogger<TopicService> _logger;

        public TopicService(ITopicRepository topicRepository, ILogger<TopicService> logger)
        {
            _topicRepository = topicRepository;
            _logger = logger;
        }

        // Для студентов - просмотр публичных топиков
        public async Task<IEnumerable<TopicListDto>> GetPublicTopicsAsync()
        {
            var topics = await _topicRepository.GetPublicTopicsAsync();
            return await MapToTopicListDtoAsync(topics);
        }

        public async Task<IEnumerable<TopicListDto>> GetPublicTopicsAsync(int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var topics = await _topicRepository.GetPublicTopicsAsync(skip, pageSize);
            return await MapToTopicListDtoAsync(topics);
        }

        public async Task<TopicDto?> GetPublicTopicByIdAsync(int id)
        {
            var topic = await _topicRepository.GetByIdWithDetailsAsync(id);
            
            if (topic == null || !topic.IsPublic)
            {
                return null;
            }

            return await MapToTopicDtoAsync(topic);
        }

        public async Task<int> GetPublicTopicsCountAsync()
        {
            return await _topicRepository.GetPublicTopicsCountAsync();
        }

        // Для преподавателей - работа со своими топиками
        public async Task<IEnumerable<TopicListDto>> GetTeacherTopicsAsync(int teacherId)
        {
            var topics = await _topicRepository.GetTopicsByTeacherAsync(teacherId);
            return await MapToTopicListDtoAsync(topics);
        }
        public async Task<IEnumerable<TopicListDto>> GetTeacherTopicsAsync(int teacherId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var topics = await _topicRepository.GetTopicsByTeacherAsync(teacherId, skip, pageSize);
            return await MapToTopicListDtoAsync(topics);
        }

        public async Task<TopicDto?> GetTeacherTopicByIdAsync(int id, int teacherId)
        {
            var topic = await _topicRepository.GetByIdWithDetailsAsync(id);
            
            if (topic == null || topic.CreatedBy != teacherId)
            {
                return null;
            }

            return await MapToTopicDtoAsync(topic);
        }

        public async Task<int> GetTeacherTopicsCountAsync(int teacherId)
        {
            return await _topicRepository.GetTopicsCountByTeacherAsync(teacherId);
        }

        // CRUD операции для преподавателей
        public async Task<TopicDto> CreateTopicAsync(CreateTopicRequest request, int teacherId)
        {
            _logger.LogInformation($"Creating topic '{request.Title}' for teacher {teacherId}");

            var topic = new Topic
            {
                Title = request.Title,
                Description = request.Description,
                IsPublic = request.IsPublic,
                CreatedBy = teacherId,
                CreatedAt = DateTime.UtcNow
            };

            var createdTopic = await _topicRepository.CreateAsync(topic);
            
            _logger.LogInformation($"Topic {createdTopic.Id} created successfully");
            
            return await MapToTopicDtoAsync(createdTopic);
        }

        public async Task<TopicDto> UpdateTopicAsync(int id, UpdateTopicRequest request, int teacherId)
        {
            _logger.LogInformation($"Updating topic {id} for teacher {teacherId}");

            // Проверяем, что топик существует и принадлежит преподавателю
            var isOwner = await _topicRepository.IsTopicOwnedByTeacherAsync(id, teacherId);
            if (!isOwner)
            {
                throw new ArgumentException(ErrorMessages.CanEditOnlyOwnTopics);
            }

            var existingTopic = await _topicRepository.GetByIdAsync(id);
            if (existingTopic == null)
            {
                throw new ArgumentException(ErrorMessages.TopicNotFound);
            }

            // Создаем объект для обновления
            var topicToUpdate = new Topic
            {
                Id = id,
                Title = request.Title,
                Description = request.Description,
                IsPublic = request.IsPublic
            };

            var updatedTopic = await _topicRepository.UpdateAsync(topicToUpdate);
            
            _logger.LogInformation($"Topic {id} updated successfully");
            
            return await MapToTopicDtoAsync(updatedTopic);
        }

        public async Task<bool> DeleteTopicAsync(int id, int teacherId)
        {
            _logger.LogInformation($"Deleting topic {id} for teacher {teacherId}");

            // Проверяем, что топик существует и принадлежит преподавателю
            var isOwner = await _topicRepository.IsTopicOwnedByTeacherAsync(id, teacherId);
            if (!isOwner)
            {
                throw new ArgumentException(ErrorMessages.CanDeleteOnlyOwnTopics);
            }

            var result = await _topicRepository.DeleteAsync(id);
            
            if (result)
            {
                _logger.LogInformation($"Topic {id} deleted successfully");
            }
            else
            {
                _logger.LogWarning($"Failed to delete topic {id}");
            }
            
            return result;
        }

        // Проверки доступа
        public async Task<bool> CanAccessTopicAsync(int topicId, int userId, string userRole)
        {
            topicId = (int)topicId;
            userId = (int)userId;
            if (userRole == "Teacher")
            {
                // Преподаватель может видеть свои топики
                return await _topicRepository.IsTopicOwnedByTeacherAsync(topicId, userId);
            }
            else if (userRole == "Student")
            {
                // Студент может видеть только публичные топики
                return await _topicRepository.IsTopicPublicAsync(topicId);
            }

            return false;
        }

        public async Task<bool> CanEditTopicAsync(int topicId, int teacherId)
        {
            return await _topicRepository.IsTopicOwnedByTeacherAsync(topicId, teacherId);
        }

        // Приватные методы маппинга
        private async Task<TopicDto> MapToTopicDtoAsync(Topic topic)
        {
            var materialsCount = await _topicRepository.GetMaterialsCountAsync(topic.Id);
            var exercisesCount = await _topicRepository.GetExercisesCountAsync(topic.Id);

            return new TopicDto
            {
                Id = topic.Id,
                Title = topic.Title,
                Description = topic.Description,
                CreatedAt = topic.CreatedAt,
                IsPublic = topic.IsPublic,
                CreatedBy = topic.CreatedBy,
                CreatedByName = $"{topic.CreatedByUser?.Name} {topic.CreatedByUser?.Surname}".Trim(),
                MaterialsCount = materialsCount,
                ExercisesCount = exercisesCount
            };
        }

        private async Task<IEnumerable<TopicListDto>> MapToTopicListDtoAsync(IEnumerable<Topic> topics)
        {
            var result = new List<TopicListDto>();

            foreach (var topic in topics)
            {
                var materialsCount = await _topicRepository.GetMaterialsCountAsync(topic.Id);
                var exercisesCount = await _topicRepository.GetExercisesCountAsync(topic.Id);

                result.Add(new TopicListDto
                {
                    Id = topic.Id,
                    Title = topic.Title,
                    Description = topic.Description,
                    CreatedAt = topic.CreatedAt,
                    IsPublic = topic.IsPublic,
                    CreatedByName = $"{topic.CreatedByUser?.Name} {topic.CreatedByUser?.Surname}".Trim(),
                    MaterialsCount = materialsCount,
                    ExercisesCount = exercisesCount
                });
            }

            return result;
        }
    }
}
