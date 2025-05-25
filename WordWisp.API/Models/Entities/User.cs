using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Entities
{
    public enum UserRole
    {
        Student = 1,
        Teacher = 2,
        Admin = 3
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.Student;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationCodeExpiry { get; set; }
    }
}
