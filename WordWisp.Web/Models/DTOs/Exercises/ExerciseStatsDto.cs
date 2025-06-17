namespace WordWisp.Web.Models.DTOs.Exercises
{
    public class ExerciseStatsDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseTitle { get; set; } = "";
        public int TotalAttempts { get; set; }
        public int CompletedAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public decimal BestScore { get; set; }
        public decimal PassingScore { get; set; }
        public DateTime? LastAttemptDate { get; set; }
        public List<UserExerciseAttemptDto> RecentAttempts { get; set; } = new();
    }

    public class UserExerciseAttemptDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int ExerciseId { get; set; }
        public string ExerciseTitle { get; set; } = "";
        public decimal Score { get; set; }
        public decimal MaxPossibleScore { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
        public List<UserExerciseAnswerDto> UserAnswers { get; set; } = new();
    }

    public class UserExerciseAnswerDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public string? AnswerText { get; set; }
        public int? SelectedAnswerId { get; set; }
        public string? SelectedAnswerText { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
        public DateTime AnsweredAt { get; set; }
    }
}
