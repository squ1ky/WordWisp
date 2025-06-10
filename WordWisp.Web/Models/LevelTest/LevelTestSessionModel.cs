public class LevelTestSessionModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime StartedAt { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeLimitMinutes { get; set; }
    public string Status { get; set; } = "";
    public int CurrentQuestionNumber { get; set; }
    public string CurrentSection { get; set; } = "";

    public int ProgressPercentage
    {
        get
        {
            if (TotalQuestions == 0) return 0;
            return Math.Min(100, (int)((double)CurrentQuestionNumber / TotalQuestions * 100));
        }
    }

    public TimeSpan TimeRemaining
    {
        get
        {
            var startTime = StartedAt.Kind == DateTimeKind.Utc ? StartedAt : StartedAt.ToUniversalTime();
            var endTime = startTime.AddMinutes(TimeLimitMinutes);
            var remaining = endTime - DateTime.UtcNow;

            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public bool IsTimeUp => TimeRemaining <= TimeSpan.Zero;
}
