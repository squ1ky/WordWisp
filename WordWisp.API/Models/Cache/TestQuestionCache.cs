using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Models.Cache
{
    public class TestQuestionCache
    {
        public Dictionary<EnglishLevel, List<LevelTestQuestion>> GrammarQuestions { get; set; } = new();
        public Dictionary<EnglishLevel, List<LevelTestQuestion>> VocabularyQuestions { get; set; } = new();
        public Dictionary<EnglishLevel, List<LevelTestQuestion>> ReadingQuestions { get; set; } = new();
    }
}
