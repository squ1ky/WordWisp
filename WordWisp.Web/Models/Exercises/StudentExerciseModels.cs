using System.ComponentModel.DataAnnotations;
using WordWisp.Web.Models.Enums;

namespace WordWisp.Web.Models.Exercises
{
    public class TakeExerciseInputModel
    {
        public int ExerciseId { get; set; }
        public int AttemptId { get; set; }
        public List<StudentAnswerInputModel> Answers { get; set; } = new();
    }

    public class StudentAnswerInputModel
    {
        [Required]
        public int QuestionId { get; set; }
        
        public List<int> SelectedAnswerIds { get; set; } = new(); // Для multiple choice
        public string? TextAnswer { get; set; } // Для fill-in-blank, essay
    }

    public class ExerciseAttemptDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int UserId { get; set; }
        public decimal Score { get; set; }
        public decimal MaxPossibleScore { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
        public DateTime TimeLimit { get; set; }
    }
}
