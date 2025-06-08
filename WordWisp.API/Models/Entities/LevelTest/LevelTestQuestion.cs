using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Entities.LevelTest
{
    public class LevelTestQuestion
    {
        public int Id { get; set; }

        [Required]
        public QuestionSection Section { get; set; }

        [Required]
        [StringLength(1000)]
        public string QuestionText { get; set; } = "";

        [StringLength(2000)]
        public string? ReadingPassage { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionA { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string OptionB { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string OptionC { get; set; } = "";

        [Required]
        [StringLength(200)]
        public string OptionD { get; set; } = "";

        [Required]
        public string CorrectAnswer { get; set; } = "";

        public EnglishLevel Difficulty { get; set; }
        public int OrderInSection { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
