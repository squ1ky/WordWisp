using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Enums;

namespace WordWisp.API.Models.Requests.Materials
{
    public class CreateMaterialWithFileRequest
    {
        [Required(ErrorMessage = "Название материала обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Тип материала обязателен")]
        public MaterialType MaterialType { get; set; }

        [Required(ErrorMessage = "Файл обязателен")]
        public IFormFile File { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        public int Order { get; set; } = 0;

        [Required(ErrorMessage = "ID топика обязателен")]
        public int TopicId { get; set; }
    }
}
