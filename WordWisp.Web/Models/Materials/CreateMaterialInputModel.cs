using System.ComponentModel.DataAnnotations;
using WordWisp.Web.Models.Enums;

namespace WordWisp.Web.Models.Materials
{
    public class CreateMaterialInputModel
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Тип материала обязателен")]
        public MaterialType MaterialType { get; set; }

        // Для текстового материала
        public string? Content { get; set; }

        // Для видео материала
        [Url(ErrorMessage = "Некорректная ссылка на видео")]
        public string? ExternalUrl { get; set; }

        // Для файлов (изображения/аудио)
        public IFormFile? File { get; set; }

        public bool IsPublic { get; set; } = false;

        public int Order { get; set; } = 0;

        public int TopicId { get; set; }
    }

    public class EditMaterialInputModel
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        // Для текстового материала
        public string? Content { get; set; }

        // Для видео материала
        [Url(ErrorMessage = "Некорректная ссылка на видео")]
        public string? ExternalUrl { get; set; }

        // Для файлов (изображения/аудио)
        public IFormFile? File { get; set; }

        public bool IsPublic { get; set; } = false;

        public int Order { get; set; } = 0;

        // Для отображения текущего файла
        public string? CurrentFilePath { get; set; }
        public string? CurrentFileName { get; set; }
    }
}
