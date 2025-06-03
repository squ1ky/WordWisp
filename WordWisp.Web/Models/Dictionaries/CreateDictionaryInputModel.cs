using System.ComponentModel.DataAnnotations;

namespace WordWisp.Web.Models.Dictionaries
{
    public class CreateDictionaryInputModel
    {
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;
    }
}

