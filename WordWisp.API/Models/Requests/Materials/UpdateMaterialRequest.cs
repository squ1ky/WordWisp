using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.Requests.Materials
{
    public class UpdateMaterialRequest
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        // Для текстового материала
        public string? Content { get; set; }

        // Для видео материала
        [Url(ErrorMessage = "Некорректная ссылка на видео")]
        public string? ExternalUrl { get; set; }

        public bool IsPublic { get; set; } = false;

        public int Order { get; set; } = 0;
    }
}
