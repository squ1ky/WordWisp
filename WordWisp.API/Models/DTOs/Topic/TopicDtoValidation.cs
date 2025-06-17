using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.DTOs.Topics
{
    public class CreateTopicDtoValidation
    {
        [Required(ErrorMessage = "Название топика обязательно")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Название должно содержать от 3 до 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не может содержать более 1000 символов")]
        public string? Description { get; set; }
    }
}
