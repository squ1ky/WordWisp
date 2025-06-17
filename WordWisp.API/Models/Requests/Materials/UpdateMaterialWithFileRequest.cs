using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Requests.Materials
{
    public class UpdateMaterialWithFileRequest
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        public string? Content { get; set; }

        [Url(ErrorMessage = "Некорректная ссылка на видео")]
        public string? ExternalUrl { get; set; }

        [Required(ErrorMessage = "Файл обязателен")]
        public IFormFile File { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        public int Order { get; set; } = 0;
    }
}
