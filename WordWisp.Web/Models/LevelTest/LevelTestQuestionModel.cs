namespace WordWisp.Web.Models.LevelTest
{
    public class LevelTestQuestionModel
    {
        public int Id { get; set; }
        public string Section { get; set; } = "";
        public string QuestionText { get; set; } = "";
        public string? ReadingPassage { get; set; }
        public string OptionA { get; set; } = "";
        public string OptionB { get; set; } = "";
        public string OptionC { get; set; } = "";
        public string OptionD { get; set; } = "";
        public int QuestionNumber { get; set; }

        public string? SelectedAnswer { get; set; }

        public bool IsReadingSection => Section.Equals("Reading", StringComparison.OrdinalIgnoreCase);
    }
}
