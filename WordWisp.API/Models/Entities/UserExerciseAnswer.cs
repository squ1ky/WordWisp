using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWisp.API.Models.Entities
{
    public class UserExerciseAnswer
    {
        [Key]
        public int Id { get; set; }

        // Связь с попыткой пользователя
        [Required]
        public int UserAttemptId { get; set; }
        
        [ForeignKey(nameof(UserAttemptId))]
        public UserExerciseAttempt UserAttempt { get; set; } = null!;

        // Связь с вопросом
        [Required]
        public int QuestionId { get; set; }
        
        [ForeignKey(nameof(QuestionId))]
        public ExerciseQuestion Question { get; set; } = null!;

        // Ответ пользователя
        public string? AnswerText { get; set; }
        public int? SelectedAnswerId { get; set; } // для множественного выбора
        public bool? IsCorrect { get; set; }
        public decimal PointsEarned { get; set; } = 0;

        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
}
