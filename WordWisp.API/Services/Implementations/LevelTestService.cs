using WordWisp.API.Data.Repositories.Interfaces;
using WordWisp.API.Models.DTOs.LevelTest;
using WordWisp.API.Models.Entities.LevelTest;
using WordWisp.API.Services.Interfaces;
using WordWisp.API.Models.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace WordWisp.API.Services.Implementations
{
    public class LevelTestService : ILevelTestService
    {
        private readonly ILevelTestRepository _repository;
        private readonly IAdaptiveTestingService _adaptiveService;
        private readonly IMemoryCache _cache;

        public LevelTestService(ILevelTestRepository repository,
                                IAdaptiveTestingService adaptiveService,
                                IMemoryCache cache)
        {
            _repository = repository;
            _adaptiveService = adaptiveService;
            _cache = cache;
        }

        public async Task<LevelTestSessionDto?> StartTestAsync(int userId)
        {
            if (!await CanStartNewTestAsync(userId))
                return null;

            var existingTest = await _repository.GetActiveTestAsync(userId);
            if (existingTest != null)
                return null;

            var test = await _repository.CreateTestAsync(userId);

            await GetOrCreateQuestionCacheAsync(test.Id);

            return new LevelTestSessionDto
            {
                Id = test.Id,
                UserId = test.UserId,
                StartedAt = test.StartedAt,
                TotalQuestions = test.TotalQuestions,
                TimeLimitMinutes = test.TimeLimitMinutes,
                Status = test.Status.ToString(),
                CurrentQuestionNumber = 1,
                CurrentSection = QuestionSection.Grammar.ToString()
            };
        }

        public async Task<LevelTestQuestionDto?> GetNextQuestionAsync(int testId, QuestionSection section, int userId)
        {
            var test = await _repository.GetTestByIdAsync(testId, userId);
            if (test == null || test.Status != TestStatus.Active)
                return null;

            var questionCache = await GetOrCreateQuestionCacheAsync(testId);
            var answeredIds = await _repository.GetAnsweredQuestionIdsAsync(testId, section);

            var question = await GetQuestionFromCache(questionCache, section, testId, answeredIds);
            if (question == null)
                return null;

            var questionNumber = await GetCurrentQuestionNumberAsync(testId, section);

            return new LevelTestQuestionDto
            {
                Id = question.Id,
                Section = question.Section.ToString(),
                QuestionText = question.QuestionText,
                ReadingPassage = question.ReadingPassage?.Content,
                OptionA = question.OptionA,
                OptionB = question.OptionB,
                OptionC = question.OptionC,
                OptionD = question.OptionD,
                OrderInSection = question.OrderInSection,
                QuestionNumber = questionNumber
            };
        }

        public async Task<bool> SubmitAnswerAsync(int testId, int questionId, string selectedAnswer, int userId)
        {
            var test = await _repository.GetTestByIdAsync(testId, userId);
            if (test == null || test.Status != TestStatus.Active)
                return false;

            var question = await _repository.GetQuestionByIdAsync(questionId);
            if (question == null)
                return false;

            var isCorrect = question.CorrectAnswer.Equals(selectedAnswer, StringComparison.OrdinalIgnoreCase);
            var questionOrder = await _repository.GetNextQuestionOrderAsync(testId);
            var estimatedLevel = await _adaptiveService.UpdateEstimatedLevelAsync(testId, isCorrect, question.Difficulty);

            var answer = new LevelTestAnswer
            {
                LevelTestId = testId,
                QuestionId = questionId,
                SelectedAnswer = selectedAnswer.ToUpper(),
                IsCorrect = isCorrect,
                AnsweredAt = DateTime.UtcNow,
                QuestionDifficulty = question.Difficulty,
                EstimatedUserLevel = estimatedLevel,
                QuestionOrder = questionOrder
            };

            await _repository.SaveAnswerAsync(answer);
            return true;
        }

        public async Task<LevelTestResultDto?> CompleteTestAsync(int testId, int userId)
        {
            var test = await _repository.GetTestByIdAsync(testId, userId);
            if (test == null || test.Status != TestStatus.Active)
                return null;

            var answers = await _repository.GetTestAnswersAsync(testId);

            const int EXPECTED_GRAMMAR_QUESTIONS = 50;
            const int EXPECTED_VOCABULARY_QUESTIONS = 50;
            const int EXPECTED_READING_QUESTIONS = 10;

            var grammarAnswers = answers.Where(a => a.Question.Section == QuestionSection.Grammar).ToList();
            var vocabularyAnswers = answers.Where(a => a.Question.Section == QuestionSection.Vocabulary).ToList();
            var readingAnswers = answers.Where(a => a.Question.Section == QuestionSection.Reading).ToList();

            var grammarScore = grammarAnswers.Count(a => a.IsCorrect);
            var vocabularyScore = vocabularyAnswers.Count(a => a.IsCorrect);
            var readingScore = readingAnswers.Count(a => a.IsCorrect);
            var totalScore = grammarScore + vocabularyScore + readingScore;

            var grammarPercentage = SafePercentage(grammarScore, EXPECTED_GRAMMAR_QUESTIONS);
            var vocabularyPercentage = SafePercentage(vocabularyScore, EXPECTED_VOCABULARY_QUESTIONS);
            var readingPercentage = SafePercentage(readingScore, EXPECTED_READING_QUESTIONS);

            var overallPercentage = CalculateWeightedAverage(grammarPercentage, vocabularyPercentage, readingPercentage);
            var level = DetermineEnglishLevel(overallPercentage);

            test.GrammarScore = grammarScore;
            test.VocabularyScore = vocabularyScore;
            test.ReadingScore = readingScore;
            test.TotalScore = totalScore;
            test.DeterminedLevel = level;
            test.CompletedAt = DateTime.UtcNow;
            test.Status = TestStatus.Completed;

            await _repository.UpdateTestAsync(test);

            return new LevelTestResultDto
            {
                TestId = test.Id,
                EnglishLevel = level.ToString(),
                TotalScore = totalScore,
                GrammarScore = grammarScore,
                VocabularyScore = vocabularyScore,
                ReadingScore = readingScore,
                GrammarPercentage = grammarPercentage,
                VocabularyPercentage = vocabularyPercentage,
                ReadingPercentage = readingPercentage,
                OverallPercentage = overallPercentage,
                CompletedAt = test.CompletedAt.Value,
                Recommendations = GenerateRecommendations(grammarScore, vocabularyScore, readingScore, level, grammarAnswers.Count, vocabularyAnswers.Count, readingAnswers.Count),
                StudyAreas = GenerateStudyAreas(grammarScore, vocabularyScore, readingScore)
            };
        }

        private double CalculateWeightedAverage(double grammarPercentage, double vocabularyPercentage, double readingPercentage)
        {
            const double GRAMMAR_WEIGHT = 50.0 / 110.0;     // 45.45%
            const double VOCABULARY_WEIGHT = 50.0 / 110.0;  // 45.45%
            const double READING_WEIGHT = 10.0 / 110.0;     // 9.09%

            return (grammarPercentage * GRAMMAR_WEIGHT) +
                   (vocabularyPercentage * VOCABULARY_WEIGHT) +
                   (readingPercentage * READING_WEIGHT);
        }


        public async Task<bool> CanStartNewTestAsync(int userId)
        {
            var lastTestDate = await _repository.GetLastTestDateAsync(userId);
            if (lastTestDate == null)
                return true;

            return DateTime.UtcNow.Subtract(lastTestDate.Value).TotalDays >= 30;
        }

        public async Task<LevelTestSessionDto?> GetActiveTestAsync(int userId)
        {
            var test = await _repository.GetActiveTestAsync(userId);
            if (test == null)
                return null;

            return new LevelTestSessionDto
            {
                Id = test.Id,
                UserId = test.UserId,
                StartedAt = test.StartedAt,
                TotalQuestions = test.TotalQuestions,
                TimeLimitMinutes = test.TimeLimitMinutes,
                Status = test.Status.ToString(),
                CurrentQuestionNumber = test.Answers.Count + 1,
                CurrentSection = DetermineCurrentSection(test.Answers.Count)
            };
        }

        private async Task<int> GetCurrentQuestionNumberAsync(int testId, QuestionSection section)
        {
            var answeredInSection = await _repository.GetAnsweredQuestionIdsAsync(testId, section);
            return answeredInSection.Count + 1;
        }

        private EnglishLevel DetermineEnglishLevel(double percentage)
        {
            return percentage switch
            {
                >= 96 => EnglishLevel.C2,
                >= 81 => EnglishLevel.C1,
                >= 66 => EnglishLevel.B2,
                >= 51 => EnglishLevel.B1,
                >= 36 => EnglishLevel.A2,
                _ => EnglishLevel.A1
            };
        }

        private string DetermineCurrentSection(int answeredCount)
        {
            return answeredCount switch
            {
                < 50 => QuestionSection.Grammar.ToString(),
                < 100 => QuestionSection.Vocabulary.ToString(),
                _ => QuestionSection.Reading.ToString()
            };
        }

        private List<string> GenerateRecommendations(int grammarScore, int vocabularyScore, int readingScore, EnglishLevel level, int grammarAnswered, int vocabularyAnswered, int readingAnswered)
        {
            var recommendations = new List<string>();

            if (grammarScore < 25) recommendations.Add("Уделите особое внимание изучению грамматики");
            if (vocabularyScore < 25) recommendations.Add("Расширяйте словарный запас через чтение и карточки");
            if (readingScore < 5) recommendations.Add("Больше читайте на английском языке");

            var totalMissed = (50 - grammarAnswered) + (50 - vocabularyAnswered) + (10 - readingAnswered);
            if (totalMissed > 20)
                recommendations.Add("В следующий раз постарайтесь ответить на все вопросы для более точной оценки");

            recommendations.Add($"Изучать материалы уровня {level}");

            return recommendations;
        }


        private List<string> GenerateStudyAreas(int grammarScore, int vocabularyScore, int readingScore)
        {
            var areas = new List<string>();

            if (grammarScore < 30) areas.Add("Grammar");
            if (vocabularyScore < 30) areas.Add("Vocabulary");
            if (readingScore < 6) areas.Add("Reading");

            return areas;
        }

        public async Task<List<LevelTestResultDto>> GetTestHistoryAsync(int userId)
        {
            var tests = await _repository.GetUserTestHistoryAsync(userId);

            var history = new List<LevelTestResultDto>();

            foreach (var test in tests)
            {
                var testAnswers = await _repository.GetTestAnswersAsync(test.Id);

                var grammarAnswers = testAnswers.Where(a => a.Question.Section == QuestionSection.Grammar).ToList();
                var vocabularyAnswers = testAnswers.Where(a => a.Question.Section == QuestionSection.Vocabulary).ToList();
                var readingAnswers = testAnswers.Where(a => a.Question.Section == QuestionSection.Reading).ToList();

                var result = new LevelTestResultDto
                {
                    TestId = test.Id,
                    EnglishLevel = test.DeterminedLevel?.ToString() ?? "Не определен",
                    TotalScore = test.TotalScore ?? 0,
                    GrammarScore = test.GrammarScore ?? 0,
                    VocabularyScore = test.VocabularyScore ?? 0,
                    ReadingScore = test.ReadingScore ?? 0,
                    GrammarPercentage = SafePercentage(test.GrammarScore ?? 0, 50),
                    VocabularyPercentage = SafePercentage(test.VocabularyScore ?? 0, 50),
                    ReadingPercentage = SafePercentage(test.ReadingScore ?? 0, 10),
                    OverallPercentage = CalculateWeightedAverage(
                        SafePercentage(test.GrammarScore ?? 0, 50),
                        SafePercentage(test.VocabularyScore ?? 0, 50),
                        SafePercentage(test.ReadingScore ?? 0, 10)
                    ),
                    CompletedAt = test.CompletedAt ?? DateTime.MinValue,
                    Recommendations = GenerateRecommendations(test.GrammarScore ?? 0, test.VocabularyScore ?? 0, test.ReadingScore ?? 0, test.DeterminedLevel ?? EnglishLevel.A1, grammarAnswers.Count, vocabularyAnswers.Count, readingAnswers.Count),
                    StudyAreas = GenerateStudyAreas(test.GrammarScore ?? 0, test.VocabularyScore ?? 0, test.ReadingScore ?? 0)
                };

                history.Add(result);
            }

            return history.OrderByDescending(h => h.CompletedAt).ToList();
        }

        private double SafePercentage(int score, int total)
        {
            if (total == 0) return 0;

            var percentage = (double)score / total * 100;

            if (double.IsNaN(percentage) || double.IsInfinity(percentage))
                return 0;

            return Math.Round(percentage, 2);
        }

        public async Task<TestQuestionCache> GetOrCreateQuestionCacheAsync(int testId)
        {
            var cacheKey = $"test_{testId}_questions";

            if (!_cache.TryGetValue(cacheKey, out TestQuestionCache? cachedData))
            {
                cachedData = new TestQuestionCache
                {
                    GrammarQuestions = await _repository.GetQuestionsBySectionGroupedByLevelAsync(QuestionSection.Grammar),
                    VocabularyQuestions = await _repository.GetQuestionsBySectionGroupedByLevelAsync(QuestionSection.Vocabulary),
                    ReadingQuestions = await _repository.GetQuestionsBySectionGroupedByLevelAsync(QuestionSection.Reading),

                    ReadingPassages = await _repository.GetReadingPassagesWithQuestionsAsync()
                };

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3),
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.High
                };

                _cache.Set(cacheKey, cachedData, cacheOptions);
            }

            return cachedData;
        }

        public async Task InvalidateTestCacheAsync(int testId)
        {
            var cacheKey = $"test_{testId}_questions";
            _cache.Remove(cacheKey);
        }

        private async Task<LevelTestQuestion?> GetQuestionFromCache(
            TestQuestionCache cache,
            QuestionSection section,
            int testId,
            List<int> answeredIds)
        {
            if (section == QuestionSection.Reading)
            {
                return await GetReadingQuestionFromCache(cache, testId, answeredIds);
            }

            var sectionQuestions = section switch
            {
                QuestionSection.Grammar => cache.GrammarQuestions,
                QuestionSection.Vocabulary => cache.VocabularyQuestions,
                _ => new Dictionary<EnglishLevel, List<LevelTestQuestion>>()
            };

            var currentLevel = await _adaptiveService.GetCurrentEstimatedLevelAsync(testId, section);

            if (sectionQuestions.ContainsKey(currentLevel))
            {
                var availableQuestions = sectionQuestions[currentLevel]
                    .Where(q => !answeredIds.Contains(q.Id))
                    .ToList();

                if (availableQuestions.Any())
                {
                    return availableQuestions.OrderBy(q => Guid.NewGuid()).First();
                }
            }

            return FindClosestLevelQuestionInCache(sectionQuestions, currentLevel, answeredIds);
        }

        private LevelTestQuestion? FindClosestLevelQuestionInCache(
            Dictionary<EnglishLevel, List<LevelTestQuestion>> questions,
            EnglishLevel targetLevel,
            List<int> answeredIds)
        {
            var levels = new List<EnglishLevel>();

            for (int i = 0; i <= 2; i++)
            {
                if (targetLevel + i <= EnglishLevel.C2)
                    levels.Add(targetLevel + i);
                if (targetLevel - i >= EnglishLevel.A1)
                    levels.Add(targetLevel - i);
            }

            foreach (var level in levels)
            {
                if (questions.ContainsKey(level))
                {
                    var availableQuestions = questions[level]
                        .Where(q => !answeredIds.Contains(q.Id))
                        .ToList();

                    if (availableQuestions.Any())
                    {
                        return availableQuestions.OrderBy(q => Guid.NewGuid()).First();
                    }
                }
            }

            return null;
        }

        private async Task<LevelTestQuestion?> GetReadingQuestionFromCache(
                TestQuestionCache cache,
                int testId,
                List<int> answeredIds)
        {
            if (!cache.ReadingPassages.Any())
            {
                return null;
            }

            var availablePassages = cache.ReadingPassages
                .Where(p => p.Questions.Any(q => !answeredIds.Contains(q.Id)))
                .ToList();

            if (!availablePassages.Any())
            {
                return null;
            }

            var currentPassageId = await _repository.GetCurrentReadingPassageIdAsync(testId);
            ReadingPassage? currentPassage = null;

            if (currentPassageId.HasValue)
            {
                currentPassage = availablePassages.FirstOrDefault(p => p.Id == currentPassageId.Value);
            }

            if (currentPassage == null || !currentPassage.Questions.Any(q => !answeredIds.Contains(q.Id)))
            {
                currentPassage = availablePassages.OrderBy(p => Guid.NewGuid()).First();
                await _repository.SaveCurrentReadingPassageIdAsync(testId, currentPassage.Id);
            }

            var nextQuestion = currentPassage.Questions
                .Where(q => !answeredIds.Contains(q.Id))
                .OrderBy(q => q.OrderInSection)
                .FirstOrDefault();

            return nextQuestion;
        }
    }
}
