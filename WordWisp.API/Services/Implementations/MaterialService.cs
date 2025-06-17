using WordWisp.API.Constants;
using WordWisp.API.Models.DTOs.Materials;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Enums;
using WordWisp.API.Models.Requests.Materials;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<MaterialService> _logger;

        public MaterialService(
            IMaterialRepository materialRepository,
            ITopicRepository topicRepository,
            IFileService fileService,
            ILogger<MaterialService> logger)
        {
            _materialRepository = materialRepository;
            _topicRepository = topicRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialRequest request, int userId)
        {
            // Проверяем доступ к топику
            var topic = await _topicRepository.GetByIdAsync(request.TopicId);
            if (topic == null)
                throw new ArgumentException(ErrorMessages.TopicNotFound);

            if (topic.CreatedBy != userId)
                throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);

            // Валидируем данные в зависимости от типа материала
            ValidateMaterialRequest(request);

            // Получаем следующий порядковый номер
            if (request.Order == 0)
            {
                request.Order = await _materialRepository.GetMaxOrderByTopicIdAsync(request.TopicId) + 1;
            }

            var material = new Material
            {
                Title = request.Title,
                Description = request.Description,
                MaterialType = request.MaterialType,
                Content = request.Content,
                ExternalUrl = request.ExternalUrl,
                IsPublic = request.IsPublic,
                Order = request.Order,
                TopicId = request.TopicId
            };

            var createdMaterial = await _materialRepository.CreateAsync(material);
            return MapToDto(createdMaterial, topic.Title);
        }

        public async Task<MaterialDto> CreateMaterialWithFileAsync(CreateMaterialRequest request, IFormFile file, int userId)
        {
            // Проверяем доступ к топику
            var topic = await _topicRepository.GetByIdAsync(request.TopicId);
            if (topic == null)
                throw new ArgumentException(ErrorMessages.TopicNotFound);

            if (topic.CreatedBy != userId)
                throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);

            // Валидируем файл
            ValidateFile(file, request.MaterialType);

            // Сохраняем файл
            var fileInfo = await _fileService.SaveFileAsync(file, GetFileDirectory(request.MaterialType));

            // Получаем следующий порядковый номер
            if (request.Order == 0)
            {
                request.Order = await _materialRepository.GetMaxOrderByTopicIdAsync(request.TopicId) + 1;
            }

            var material = new Material
            {
                Title = request.Title,
                Description = request.Description,
                MaterialType = request.MaterialType,
                Content = request.Content,
                FilePath = fileInfo.FilePath,
                FileSize = fileInfo.FileSize,
                MimeType = fileInfo.MimeType,
                OriginalFileName = fileInfo.OriginalFileName,
                IsPublic = request.IsPublic,
                Order = request.Order,
                TopicId = request.TopicId
            };

            var createdMaterial = await _materialRepository.CreateAsync(material);
            return MapToDto(createdMaterial, topic.Title);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(int id, int? userId = null)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(id);
            if (material == null) return null;

            // Проверяем доступ
            if (!material.IsPublic && (userId == null || material.Topic.CreatedBy != userId))
                return null;

            return MapToDto(material, material.Topic.Title);
        }

        public async Task<List<MaterialDto>> GetMaterialsByTopicIdAsync(int topicId, int? userId = null)
        {
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null) return new List<MaterialDto>();

            List<Material> materials;

            // Если пользователь - владелец топика, показываем все материалы
            if (userId.HasValue && topic.CreatedBy == userId.Value)
            {
                materials = await _materialRepository.GetByTopicIdAsync(topicId);
            }
            else
            {
                // Иначе только публичные
                materials = await _materialRepository.GetPublicByTopicIdAsync(topicId);
            }

            return materials.Select(m => MapToDto(m, topic.Title)).ToList();
        }

        public async Task<MaterialDto> UpdateMaterialAsync(int id, UpdateMaterialRequest request, int userId)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(id);
            if (material == null)
                throw new ArgumentException(ErrorMessages.MaterialNotFound);

            if (material.Topic.CreatedBy != userId)
                throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);

            // Обновляем свойства
            material.Title = request.Title;
            material.Description = request.Description;
            material.Content = request.Content;
            material.ExternalUrl = request.ExternalUrl;
            material.IsPublic = request.IsPublic;
            material.Order = request.Order;

            var updatedMaterial = await _materialRepository.UpdateAsync(material);
            return MapToDto(updatedMaterial, material.Topic.Title);
        }

        public async Task<MaterialDto> UpdateMaterialWithFileAsync(int id, UpdateMaterialRequest request, IFormFile file, int userId)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(id);
            if (material == null)
                throw new ArgumentException(ErrorMessages.MaterialNotFound);

            if (material.Topic.CreatedBy != userId)
                throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);

            // Валидируем файл
            ValidateFile(file, material.MaterialType);

            // Удаляем старый файл если есть
            if (!string.IsNullOrEmpty(material.FilePath))
            {
                await _fileService.DeleteFileAsync(material.FilePath);
            }

            // Сохраняем новый файл
            var fileInfo = await _fileService.SaveFileAsync(file, GetFileDirectory(material.MaterialType));

            // Обновляем свойства
            material.Title = request.Title;
            material.Description = request.Description;
            material.Content = request.Content;
            material.ExternalUrl = request.ExternalUrl;
            material.FilePath = fileInfo.FilePath;
            material.FileSize = fileInfo.FileSize;
            material.MimeType = fileInfo.MimeType;
            material.OriginalFileName = fileInfo.OriginalFileName;
            material.IsPublic = request.IsPublic;
            material.Order = request.Order;

            var updatedMaterial = await _materialRepository.UpdateAsync(material);
            return MapToDto(updatedMaterial, material.Topic.Title);
        }

        public async Task DeleteMaterialAsync(int id, int userId)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(id);
            if (material == null)
                throw new ArgumentException(ErrorMessages.MaterialNotFound);

            if (material.Topic.CreatedBy != userId)
                throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);

            // Удаляем файл если есть
            if (!string.IsNullOrEmpty(material.FilePath))
            {
                await _fileService.DeleteFileAsync(material.FilePath);
            }

            await _materialRepository.DeleteAsync(id);
        }

        public async Task<bool> CanAccessMaterialAsync(int materialId, int userId)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(materialId);
            if (material == null) return false;

            return material.IsPublic || material.Topic.CreatedBy == userId;
        }

        public async Task<bool> CanModifyMaterialAsync(int materialId, int userId)
        {
            var material = await _materialRepository.GetByIdWithTopicAsync(materialId);
            if (material == null) return false;

            return material.Topic.CreatedBy == userId;
        }

        private void ValidateMaterialRequest(CreateMaterialRequest request)
        {
            switch (request.MaterialType)
            {
                case MaterialType.Text:
                    if (string.IsNullOrEmpty(request.Content))
                        throw new ArgumentException("Содержимое текстового материала обязательно");
                    break;
                case MaterialType.Video:
                    if (string.IsNullOrEmpty(request.ExternalUrl))
                        throw new ArgumentException("Ссылка на видео обязательна");
                    break;
                case MaterialType.Image:
                case MaterialType.Audio:
                    // Для изображений и аудио файл будет загружен отдельно
                    break;
            }
        }

        private void ValidateFile(IFormFile file, MaterialType materialType)
        {
            var allowedTypes = GetAllowedMimeTypes(materialType);
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                throw new ArgumentException($"Неподдерживаемый тип файла для {materialType}");
            }

            var maxSize = GetMaxFileSize(materialType);
            if (file.Length > maxSize)
            {
                throw new ArgumentException($"Размер файла превышает максимально допустимый ({maxSize / (1024 * 1024)} MB)");
            }
        }

        private string[] GetAllowedMimeTypes(MaterialType materialType)
        {
            return materialType switch
            {
                MaterialType.Image => new[] { "image/jpeg", "image/png", "image/gif", "image/webp" },
                MaterialType.Audio => new[] { "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp4" },
                _ => new string[0]
            };
        }

        private long GetMaxFileSize(MaterialType materialType)
        {
            return materialType switch
            {
                MaterialType.Image => 5 * 1024 * 1024, // 5 MB
                MaterialType.Audio => 50 * 1024 * 1024, // 50 MB
                _ => 1024 * 1024 // 1 MB по умолчанию
            };
        }

        private string GetFileDirectory(MaterialType materialType)
        {
            return materialType switch
            {
                MaterialType.Image => "images",
                MaterialType.Audio => "audio",
                _ => "files"
            };
        }

        private MaterialDto MapToDto(Material material, string topicTitle)
        {
            return new MaterialDto
            {
                Id = material.Id,
                Title = material.Title,
                Description = material.Description,
                Content = material.Content,
                MaterialType = material.MaterialType,
                FilePath = material.FilePath,
                ExternalUrl = material.ExternalUrl,
                FileSize = material.FileSize,
                MimeType = material.MimeType,
                OriginalFileName = material.OriginalFileName,
                IsPublic = material.IsPublic,
                Order = material.Order,
                TopicId = material.TopicId,
                TopicTitle = topicTitle,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }
    }
}
