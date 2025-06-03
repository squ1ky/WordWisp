using WordWisp.API.Models.Entities;

namespace WordWisp.API.Entities
{
    public class Dictionary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int UserId { get; set; }
        public bool IsPublic { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Word> Words { get; set; } = new List<Word>();
    }
}
