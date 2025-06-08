using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Data.Repositories.Interfaces
{
    public interface ILevelTestRepository
    {
        Task<LevelTest?> GetActiveTestAsync(int userId);
        Task<LevelTest> CreateTestAsync(int userId);
        Task<LevelTest?> GetTestByIdAsync(int testId, int userId);
        Task<LevelTest> UpdateTestAsync(LevelTest test);

        Task<LevelTestQuestion?> GetQuestionByIdAsync(int questionId);
        Task<LevelTestQuestion?> GetQuestionByDifficultyAsync(QuestionSection section, EnglishLevel difficulty, List<int> excludeIds);
        Task<LevelTestQuestion?> GetClosestDifficultyQuestionAsync(QuestionSection section, EnglishLevel targetDifficulty, List<int> excludeIds);

        Task<LevelTestAnswer> SaveAnswerAsync(LevelTestAnswer answer);
        Task<LevelTestAnswer> UpdateAnswerAsync(LevelTestAnswer answer);
        Task<List<LevelTestAnswer>> GetTestAnswersAsync(int testId);
        Task<bool> HasAnswerAsync(int testId, int questionId);
        Task<List<int>> GetAnsweredQuestionIdsAsync(int testId, QuestionSection section);
        Task<int> GetNextQuestionOrderAsync(int testId);

        Task<List<LevelTest>> GetUserTestHistoryAsync(int userId);
        Task<DateTime?> GetLastTestDateAsync(int userId);
    }
}
