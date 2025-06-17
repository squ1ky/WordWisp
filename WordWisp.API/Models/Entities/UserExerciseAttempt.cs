using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WordWisp.API.Models.Entities
{
    public class UserExerciseAttempt
    {
        [Key]
        public int Id { get; set; }

        // Связь с пользователем
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Связь с упражнением
        [Required]
        public int ExerciseId { get; set; }
        
        [ForeignKey(nameof(ExerciseId))]
        public Exercise Exercise { get; set; } = null!;

        // Результаты попытки
        public decimal Score { get; set; } = 0; // в процентах
        public decimal MaxPossibleScore { get; set; } = 100;
        public bool IsCompleted { get; set; } = false;
        public bool IsPassed { get; set; } = false;

        // Временные метки
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public int TimeSpentSeconds { get; set; } = 0;

        // Ответы пользователя
        public ICollection<UserExerciseAnswer> UserAnswers { get; set; } = new List<UserExerciseAnswer>();
    }
}
