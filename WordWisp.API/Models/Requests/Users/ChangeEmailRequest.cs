using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Requests.Users
{
    public class ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = "";
    }
}
