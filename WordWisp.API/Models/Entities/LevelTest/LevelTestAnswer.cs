using System.ComponentModel.DataAnnotations;

namespace WordWisp.API.Models.Entities.LevelTest
{
    public class LevelTestAnswer
    {
        public int Id { get; set; }

        [Required]
        public int LevelTestId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(1)]
        public string SelectedAnswer { get; set; } = "";

        public bool IsCorrect { get; set; }
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

        public EnglishLevel QuestionDifficulty { get; set; }
        public EnglishLevel EstimatedUserLevel { get; set; }
        public int QuestionOrder { get; set; }

        public virtual LevelTest LevelTest { get; set; } = null!;
        public virtual LevelTestQuestion Question { get; set; } = null!;
    }
}
