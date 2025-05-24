using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Entities;

namespace WordWisp.API.Models.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public UserRole Role { get; set; } = UserRole.Student;
    }
}
