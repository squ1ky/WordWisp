namespace WordWisp.API.Models.DTOs.LevelTest
{
    public class LevelTestSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Status { get; set; } = "";
        public int CurrentQuestionNumber { get; set; }
        public string CurrentSection { get; set; } = "";
    }
}
