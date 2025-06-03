using System.ComponentModel.DataAnnotations;

namespace WordWisp.Web.Models.Words
{
    public class CreateWordInputModel
    {
        [Required(ErrorMessage = "Слово/фраза обязательно")]
        [StringLength(200, ErrorMessage = "Слово не должно превышать 200 символов")]
        public string Term { get; set; } = "";

        [Required(ErrorMessage = "Перевод/определение обязательно")]
        [StringLength(500, ErrorMessage = "Определение не должно превышать 500 символов")]
        public string Definition { get; set; } = "";

        [StringLength(200, ErrorMessage = "Транскрипция не должна превышать 200 символов")]
        public string? Transcription { get; set; }

        [StringLength(500, ErrorMessage = "Пример не должен превышать 500 символов")]
        public string? Example { get; set; }
    }
}

