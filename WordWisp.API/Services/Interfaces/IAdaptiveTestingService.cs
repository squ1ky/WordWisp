using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Services.Interfaces
{
    public interface IAdaptiveTestingService
    {
        Task<EnglishLevel> UpdateEstimatedLevelAsync(int testId, bool isCorrect, EnglishLevel questionDifficulty);
        Task<EnglishLevel> GetCurrentEstimatedLevelAsync(int testId, QuestionSection? section = null);
    }
}
