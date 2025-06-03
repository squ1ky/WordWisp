using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.DTOs.Dictionaries
{
    public class UpdateDictionaryRequest
    {
        [Required(ErrorMessage = "Название словаря обязательно")]
        [StringLength(200, ErrorMessage = "Название должно быть не более 200 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Описание должно быть не более 500 символов")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;
    }
}

