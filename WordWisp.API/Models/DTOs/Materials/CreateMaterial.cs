using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.DTOs.Materials
{
    public class CreateMaterialDto
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        public string? Content { get; set; }

        [Required(ErrorMessage = "Тип материала обязателен")]
        public MaterialType MaterialType { get; set; }

        public string? ExternalUrl { get; set; }

        [Required(ErrorMessage = "ID темы обязателен")]
        public int TopicId { get; set; }

        public int Order { get; set; } = 0;
    }
}
