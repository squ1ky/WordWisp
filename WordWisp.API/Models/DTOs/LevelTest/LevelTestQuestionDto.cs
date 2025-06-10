namespace WordWisp.API.Models.DTOs.LevelTest
{
    public class LevelTestQuestionDto
    {
        public int Id { get; set; }
        public string Section { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string? ReadingPassage { get; set; }
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
        public int OrderInSection { get; set; }
        public int QuestionNumber { get; set; }
    }
}
