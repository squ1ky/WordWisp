using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWisp.API.Models.Entities
{
    public class ExerciseAnswer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AnswerText { get; set; } = string.Empty;

        public string? AnswerImagePath { get; set; }
        public bool IsCorrect { get; set; } = false;
        public int Order { get; set; } = 0;

        // Связь с вопросом
        [Required]
        public int QuestionId { get; set; }
        
        [ForeignKey(nameof(QuestionId))]
        public ExerciseQuestion Question { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
