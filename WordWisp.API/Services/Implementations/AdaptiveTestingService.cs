using WordWisp.API.Data.Repositories.Interfaces;
using WordWisp.API.Models.Entities.LevelTest;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class AdaptiveTestingService : IAdaptiveTestingService
    {
        private readonly ILevelTestRepository _repository;

        public AdaptiveTestingService(ILevelTestRepository repository)
        {
            _repository = repository;
        }

        public async Task<LevelTestQuestion?> GetNextQuestionAsync(int testId, QuestionSection section)
        {
            var currentLevel = await GetCurrentEstimatedLevelAsync(testId, section);
            var answeredQuestions = await _repository.GetAnsweredQuestionIdsAsync(testId, section);

            var question = await _repository.GetQuestionByDifficultyAsync(section, currentLevel, answeredQuestions);

            if (question == null)
            {
                question = await _repository.GetClosestDifficultyQuestionAsync(section, currentLevel, answeredQuestions);
            }

            return question;
        }

        public async Task<EnglishLevel> UpdateEstimatedLevelAsync(int testId, bool isCorrect, EnglishLevel questionDifficulty)
        {
            var currentLevel = await GetCurrentEstimatedLevelAsync(testId);

            if (isCorrect)
            {
                return IncreaseLevel(currentLevel, questionDifficulty);
            }
            else
            {
                return DecreaseLevel(currentLevel, questionDifficulty);
            }
        }

        public async Task<EnglishLevel> GetCurrentEstimatedLevelAsync(int testId, QuestionSection? section = null)
        {
            var answers = await _repository.GetTestAnswersAsync(testId);

            if (section.HasValue)
            {
                answers = answers.Where(a => a.Question.Section == section.Value).ToList();
            }

            if (!answers.Any())
            {
                return EnglishLevel.B1;
            }

            var recentAnswers = answers.OrderByDescending(a => a.AnsweredAt).Take(5);
            var correctCount = recentAnswers.Count(a => a.IsCorrect);
            var totalCount = recentAnswers.Count();
            var accuracy = (double)correctCount / totalCount;

            var lastAnswer = answers.OrderByDescending(a => a.AnsweredAt).First();
            var baseLevel = lastAnswer.EstimatedUserLevel;

            if (accuracy >= 0.8) return IncreaseLevel(baseLevel);
            if (accuracy <= 0.4) return DecreaseLevel(baseLevel);

            return baseLevel;
        }

        private EnglishLevel IncreaseLevel(EnglishLevel current, EnglishLevel? questionLevel = null)
        {
            if (questionLevel.HasValue && questionLevel.Value >= current)
            {
                return current < EnglishLevel.C2 ? current + 1 : EnglishLevel.C2;
            }

            return current;
        }

        private EnglishLevel DecreaseLevel(EnglishLevel current, EnglishLevel? questionLevel = null)
        {
            if (questionLevel.HasValue && questionLevel.Value <= current)
            {
                return current > EnglishLevel.A1 ? current - 1 : EnglishLevel.A1;
            }

            return current;
        }
    }
}
