using System.ComponentModel.DataAnnotations;
using WordWisp.API.Models.Entities;

namespace WordWisp.API.Models.Requests.Users
{
    public class UpdateUserRequest
    {
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

        public UserRole Role { get; set; }
    }
}
