using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WordWisp.API.Models.Enums;

using WordWisp.API.Entities;

namespace WordWisp.API.Models.Entities
{
    public class Material
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Содержимое материала (для текстового типа)
        public string? Content { get; set; }

        // Тип материала
        [Required]
        public MaterialType MaterialType { get; set; }

        // Путь к файлу (для изображений/аудио)
        public string? FilePath { get; set; }

        // Внешняя ссылка (для видео)
        public string? ExternalUrl { get; set; }

        // Метаинформация о файле
        public long? FileSize { get; set; }
        public string? MimeType { get; set; }
        public string? OriginalFileName { get; set; }

        // Публикация материала
        public bool IsPublic { get; set; } = false;

        // Порядок отображения в теме
        public int Order { get; set; } = 0;

        // Связь с темой
        [Required]
        public int TopicId { get; set; }
        
        [ForeignKey(nameof(TopicId))]
        public Topic Topic { get; set; } = null!;

        // Даты создания и обновления
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Связь с упражнениями
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
