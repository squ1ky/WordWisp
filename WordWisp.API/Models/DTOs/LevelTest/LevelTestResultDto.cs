namespace WordWisp.API.Models.DTOs.LevelTest
{
    public class LevelTestResultDto
    {
        public int TestId { get; set; }
        public string EnglishLevel { get; set; } = "";
        public int TotalScore { get; set; }
        public int GrammarScore { get; set; }
        public int VocabularyScore { get; set; }
        public int ReadingScore { get; set; }
        public double GrammarPercentage { get; set; }
        public double VocabularyPercentage { get; set; }
        public double ReadingPercentage { get; set; }
        public double OverallPercentage { get; set; }
        public DateTime CompletedAt { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public List<string> StudyAreas { get; set; } = new();
    }
}
