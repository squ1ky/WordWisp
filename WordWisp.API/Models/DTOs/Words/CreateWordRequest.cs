using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.DTOs.Words
{
    public class CreateWordRequest
    {
        [Required(ErrorMessage = "Термин обязателен")]
        [StringLength(200, ErrorMessage = "Термин должен быть не более 200 символов")]
        public string Term { get; set; } = string.Empty;

        [Required(ErrorMessage = "Определение обязательно")]
        [StringLength(500, ErrorMessage = "Определение должно быть не более 500 символов")]
        public string Definition { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Транскрипция должна быть не более 200 символов")]
        public string? Transcription { get; set; }

        [StringLength(1000, ErrorMessage = "Пример должен быть не более 1000 символов")]
        public string? Example { get; set; }
    }
}

