﻿using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Data.Repositories.Interfaces
{
    public interface ILevelTestRepository
    {
        Task<LevelTest?> GetActiveTestAsync(int userId);
        Task<LevelTest> CreateTestAsync(int userId);
        Task<LevelTest?> GetTestByIdAsync(int testId, int userId);
        Task<LevelTest> UpdateTestAsync(LevelTest test);

        Task<LevelTestQuestion?> GetQuestionByIdAsync(int questionId);
        Task<LevelTestAnswer> SaveAnswerAsync(LevelTestAnswer answer);
        Task<List<LevelTestAnswer>> GetTestAnswersAsync(int testId);
        Task<List<int>> GetAnsweredQuestionIdsAsync(int testId, QuestionSection section);
        Task<int> GetNextQuestionOrderAsync(int testId);
        Task<List<LevelTest>> GetUserTestHistoryAsync(int userId);
        Task<DateTime?> GetLastTestDateAsync(int userId);
        Task<string?> GetLastTestLevelAsync(int userId);
        Task<int?> GetLastTestIdAsync(int userId);
        Task<Dictionary<EnglishLevel, List<LevelTestQuestion>>> GetQuestionsBySectionGroupedByLevelAsync(QuestionSection section);

        Task<List<ReadingPassage>> GetReadingPassagesWithQuestionsAsync();
        Task<int?> GetCurrentReadingPassageIdAsync(int testId);
        Task SaveCurrentReadingPassageIdAsync(int testId, int passageId);
    }
}
