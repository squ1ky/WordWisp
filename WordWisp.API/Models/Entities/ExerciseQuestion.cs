using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWisp.API.Models.Entities
{
    public class ExerciseQuestion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Question { get; set; } = string.Empty;

        public string? QuestionImagePath { get; set; }
        public string? QuestionAudioPath { get; set; }

        public int Order { get; set; } = 0;
        public int Points { get; set; } = 1;

        // Связь с упражнением
        [Required]
        public int ExerciseId { get; set; }
        
        [ForeignKey(nameof(ExerciseId))]
        public Exercise Exercise { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Варианты ответов
        public ICollection<ExerciseAnswer> Answers { get; set; } = new List<ExerciseAnswer>();
        
        // Ответы пользователей
        public ICollection<UserExerciseAnswer> UserAnswers { get; set; } = new List<UserExerciseAnswer>();
    }
}
