using System.ComponentModel.DataAnnotations;

namespace WordWisp.Web.Models.Auth
{
    public class VerifyEmailInputModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Код подтверждения обязателен")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен содержать 6 цифр")]
        public string VerificationCode { get; set; } = string.Empty;
    }
}

