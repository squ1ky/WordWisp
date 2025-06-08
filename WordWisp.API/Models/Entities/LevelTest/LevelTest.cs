using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Entities.LevelTest
{
    public class LevelTest
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int TotalQuestions { get; set; } = 110;
        public int TimeLimitMinutes { get; set; } = 120;

        public int? GrammarScore { get; set; }
        public int? VocabularyScore { get; set; }
        public int? ReadingScore { get; set; }
        public int? TotalScore { get; set; }

        public EnglishLevel? DeterminedLevel { get; set; }

        public TestStatus Status { get; set; } = TestStatus.Active;

        public virtual ICollection<LevelTestAnswer> Answers { get; set; } = new List<LevelTestAnswer>();
    }
}
