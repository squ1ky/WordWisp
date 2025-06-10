using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WordWisp.API.Data.Repositories.Interfaces;
using WordWisp.API.Models.Entities.LevelTest;

namespace WordWisp.API.Data.Repositories.Implementations
{
    public class LevelTestRepository : ILevelTestRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public LevelTestRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<LevelTest?> GetActiveTestAsync(int userId)
        {
            return await _context.LevelTests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Status == TestStatus.Active);
        }

        public async Task<LevelTest> CreateTestAsync(int userId)
        {
            var test = new LevelTest
            {
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = TestStatus.Active,
                TotalQuestions = 110,
                TimeLimitMinutes = 120
            };

            _context.LevelTests.Add(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task<LevelTest?> GetTestByIdAsync(int testId, int userId)
        {
            return await _context.LevelTests
                .Include(t => t.Answers)
                .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(t => t.Id == testId && t.UserId == userId);
        }

        public async Task<LevelTest> UpdateTestAsync(LevelTest test)
        {
            _context.LevelTests.Update(test);
            await _context.SaveChangesAsync();
            return test;
        }

        public async Task<LevelTestQuestion?> GetQuestionByIdAsync(int questionId)
        {
            return await _context.LevelTestQuestions
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<LevelTestQuestion?> GetQuestionByDifficultyAsync(QuestionSection section, EnglishLevel difficulty, List<int> excludeIds)
        {
            return await _context.LevelTestQuestions
                .Where(q => q.Section == section
                         && q.Difficulty == difficulty
                         && q.IsActive
                         && !excludeIds.Contains(q.Id))
                .OrderBy(q => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<LevelTestQuestion?> GetClosestDifficultyQuestionAsync(QuestionSection section, EnglishLevel targetDifficulty, List<int> excludeIds)
        {
            var levels = new List<EnglishLevel>();

            for (int i = 0; i <= 2; i++)
            {
                if (targetDifficulty + i <= EnglishLevel.C2)
                    levels.Add(targetDifficulty + i);
                if (targetDifficulty - i >= EnglishLevel.A1)
                    levels.Add(targetDifficulty - i);
            }

            foreach (var level in levels)
            {
                var question = await _context.LevelTestQuestions
                    .Where(q => q.Section == section
                             && q.Difficulty == level
                             && q.IsActive
                             && !excludeIds.Contains(q.Id))
                    .OrderBy(q => Guid.NewGuid())
                    .FirstOrDefaultAsync();

                if (question != null)
                    return question;
            }

            return null;
        }

        public async Task<LevelTestAnswer> SaveAnswerAsync(LevelTestAnswer answer)
        {
            var existing = await _context.LevelTestAnswers
                .FirstOrDefaultAsync(a => a.LevelTestId == answer.LevelTestId && a.QuestionId == answer.QuestionId);

            if (existing != null)
            {
                existing.SelectedAnswer = answer.SelectedAnswer;
                existing.IsCorrect = answer.IsCorrect;
                existing.AnsweredAt = DateTime.UtcNow;
                existing.QuestionDifficulty = answer.QuestionDifficulty;
                existing.EstimatedUserLevel = answer.EstimatedUserLevel;
                _context.LevelTestAnswers.Update(existing);
            }
            else
            {
                _context.LevelTestAnswers.Add(answer);
            }

            await _context.SaveChangesAsync();
            return existing ?? answer;
        }

        public async Task<LevelTestAnswer> UpdateAnswerAsync(LevelTestAnswer answer)
        {
            _context.LevelTestAnswers.Update(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task<List<LevelTestAnswer>> GetTestAnswersAsync(int testId)
        {
            return await _context.LevelTestAnswers
                .Include(a => a.Question)
                .Where(a => a.LevelTestId == testId)
                .OrderBy(a => a.QuestionOrder)
                .ToListAsync();
        }

        public async Task<bool> HasAnswerAsync(int testId, int questionId)
        {
            return await _context.LevelTestAnswers
                .AnyAsync(a => a.LevelTestId == testId && a.QuestionId == questionId);
        }

        public async Task<List<int>> GetAnsweredQuestionIdsAsync(int testId, QuestionSection section)
        {
            return await _context.LevelTestAnswers
                .Include(a => a.Question)
                .Where(a => a.LevelTestId == testId && a.Question.Section == section)
                .Select(a => a.QuestionId)
                .ToListAsync();
        }

        public async Task<int> GetNextQuestionOrderAsync(int testId)
        {
            var lastOrder = await _context.LevelTestAnswers
                .Where(a => a.LevelTestId == testId)
                .MaxAsync(a => (int?)a.QuestionOrder) ?? 0;

            return lastOrder + 1;
        }

        public async Task<List<LevelTest>> GetUserTestHistoryAsync(int userId)
        {
            return await _context.LevelTests
                .Where(t => t.UserId == userId && t.Status == TestStatus.Completed)
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastTestDateAsync(int userId)
        {
            var lastTest = await _context.LevelTests
                .Where(t => t.UserId == userId && t.Status == TestStatus.Completed)
                .OrderByDescending(t => t.CompletedAt)
                .FirstOrDefaultAsync();

            return lastTest?.CompletedAt;
        }

        public async Task<Dictionary<EnglishLevel, List<LevelTestQuestion>>> GetQuestionsBySectionGroupedByLevelAsync(QuestionSection section)
        {
            var questions = await _context.LevelTestQuestions
                .Where(q => q.Section == section && q.IsActive)
                .ToListAsync();

            return questions.GroupBy(q => q.Difficulty)
                           .ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<List<ReadingPassage>> GetReadingPassagesWithQuestionsAsync()
        {
            return await _context.ReadingPassages
                .Include(p => p.Questions.Where(q => q.IsActive))
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<ReadingPassage?> GetReadingPassageByIdAsync(int passageId)
        {
            return await _context.ReadingPassages
                .Include(p => p.Questions)
                .FirstOrDefaultAsync(p => p.Id == passageId);
        }

        public async Task<int?> GetCurrentReadingPassageIdAsync(int testId)
        {
            var cacheKey = $"test_{testId}_reading_passage";

            if (_cache?.TryGetValue(cacheKey, out int passageId) == true)
            {
                return passageId;
            }

            return null;
        }

        public async Task SaveCurrentReadingPassageIdAsync(int testId, int passageId)
        {
            var cacheKey = $"test_{testId}_reading_passage";

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3),
                Priority = CacheItemPriority.Normal
            };

            _cache?.Set(cacheKey, passageId, cacheOptions);
        }
    }
}
