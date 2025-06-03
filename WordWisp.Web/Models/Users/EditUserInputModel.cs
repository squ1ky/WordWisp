using System.ComponentModel.DataAnnotations;
using WordWisp.Web.Models.Entities;

namespace WordWisp.Web.Models.Users
{
    public class EditUserInputModel
    {
        [Required(ErrorMessage = "Имя пользователя обязательно")]
        [StringLength(50, ErrorMessage = "Имя пользователя не должно превышать 50 символов")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
        public string Surname { get; set; } = "";

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Роль обязательна")]
        public UserRole Role { get; set; }
    }

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "Текущий пароль обязателен")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "Новый пароль обязателен")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = "";
    }
}
