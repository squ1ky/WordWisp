namespace WordWisp.Web.Models.LevelTest
{
    public class LevelTestResultModel
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

        public string LevelDescription => EnglishLevel switch
        {
            "A1" => "Начальный уровень",
            "A2" => "Элементарный уровень",
            "B1" => "Средний уровень",
            "B2" => "Выше среднего",
            "C1" => "Продвинутый уровень",
            "C2" => "Профессиональный уровень",
            _ => "Не определен"
        };

        public string LevelColor => EnglishLevel switch
        {
            "A1" => "danger",
            "A2" => "warning",
            "B1" => "info",
            "B2" => "primary",
            "C1" => "success",
            "C2" => "dark",
            _ => "secondary"
        };
    }
}
