using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Entities.LevelTest
{
    public class ReadingPassage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = "";

        [Required]
        public string Content { get; set; } = "";

        public EnglishLevel Level { get; set; }

        [StringLength(100)]
        public string Topic { get; set; } = "";

        public int WordCount { get; set; }
        public int EstimatedReadingTime { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<LevelTestQuestion> Questions { get; set; } = new List<LevelTestQuestion>();
    }
}
