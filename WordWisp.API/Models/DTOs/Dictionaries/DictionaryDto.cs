namespace WordWisp.API.Models.DTOs.Dictionaries
{
    public class DictionaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int WordsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }
}
