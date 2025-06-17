using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Requests.Topics
{
    public class CreateTopicRequest
    {
        [Required(ErrorMessage = "Название топика обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}
