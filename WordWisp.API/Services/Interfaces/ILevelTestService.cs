using WordWisp.API.Models.Cache;
using WordWisp.API.Models.DTOs.LevelTest;
using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Services.Interfaces
{
    public interface ILevelTestService
    {
        Task<LevelTestSessionDto?> StartTestAsync(int userId);
        Task<LevelTestQuestionDto?> GetNextQuestionAsync(int testId, QuestionSection section, int userId);
        Task<bool> SubmitAnswerAsync(int testId, int questionId, string selectedAnswer, int userId);
        Task<LevelTestResultDto?> CompleteTestAsync(int testId, int userId);
        Task<bool> CanStartNewTestAsync(int userId);
        Task<LevelTestSessionDto?> GetActiveTestAsync(int userId);
        Task<List<LevelTestResultDto>> GetTestHistoryAsync(int userId);

        Task<TestQuestionCache> GetOrCreateQuestionCacheAsync(int testId);
        Task InvalidateTestCacheAsync(int testId);
    }
}
