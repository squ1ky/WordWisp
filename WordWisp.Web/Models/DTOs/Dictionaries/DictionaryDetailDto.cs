using WordWisp.Web.Models.DTOs.Words;

namespace WordWisp.Web.Models.DTOs.Dictionaries
{
    public class DictionaryDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public List<WordDto> Words { get; set; } = new();
    }
}
