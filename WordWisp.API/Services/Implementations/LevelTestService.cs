using WordWisp.API.Data.Repositories.Interfaces;
using WordWisp.API.Models.DTOs.LevelTest;
using WordWisp.API.Models.Entities.LevelTest;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class LevelTestService : ILevelTestService
    {
        private readonly ILevelTestRepository _repository;
        private readonly IAdaptiveTestingService _adaptiveService;

        public LevelTestService(ILevelTestRepository repository, IAdaptiveTestingService adaptiveService)
        {
            _repository = repository;
            _adaptiveService = adaptiveService;
        }

        public async Task<LevelTestSessionDto?> StartTestAsync(int userId)
        {
            if (!await CanStartNewTestAsync(userId))
                return null;

            var existingTest = await _repository.GetActiveTestAsync(userId);
            if (existingTest != null)
                return null;

            var test = await _repository.CreateTestAsync(userId);

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

            var question = await _adaptiveService.GetNextQuestionAsync(testId, section);
            if (question == null)
                return null;

            var questionNumber = await GetCurrentQuestionNumberAsync(testId, section);

            return new LevelTestQuestionDto
            {
                Id = question.Id,
                Section = question.Section.ToString(),
                QuestionText = question.QuestionText,
                ReadingPassage = question.ReadingPassage,
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

            var grammarAnswers = answers.Where(a => a.Question.Section == QuestionSection.Grammar).ToList();
            var vocabularyAnswers = answers.Where(a => a.Question.Section == QuestionSection.Vocabulary).ToList();
            var readingAnswers = answers.Where(a => a.Question.Section == QuestionSection.Reading).ToList();

            var grammarScore = grammarAnswers.Count(a => a.IsCorrect);
            var vocabularyScore = vocabularyAnswers.Count(a => a.IsCorrect);
            var readingScore = readingAnswers.Count(a => a.IsCorrect);
            var totalScore = grammarScore + vocabularyScore + readingScore;

            var overallPercentage = SafePercentage(totalScore, answers.Count);
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
                GrammarPercentage = SafePercentage(grammarScore, grammarAnswers.Count),
                VocabularyPercentage = SafePercentage(vocabularyScore, vocabularyAnswers.Count),
                ReadingPercentage = SafePercentage(readingScore, readingAnswers.Count),
                OverallPercentage = overallPercentage,
                CompletedAt = test.CompletedAt.Value,
                Recommendations = GenerateRecommendations(grammarScore, vocabularyScore, readingScore, level),
                StudyAreas = GenerateStudyAreas(grammarScore, vocabularyScore, readingScore)
            };
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

        private List<string> GenerateRecommendations(int grammarScore, int vocabularyScore, int readingScore, EnglishLevel level)
        {
            var recommendations = new List<string>();

            if (grammarScore < vocabularyScore) recommendations.Add("Улучшить знание грамматики");
            if (vocabularyScore < grammarScore) recommendations.Add("Расширить словарный запас");
            if (readingScore < (grammarScore + vocabularyScore) / 2) recommendations.Add("Практиковать чтение");

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
                    GrammarPercentage = SafePercentage(test.GrammarScore ?? 0, grammarAnswers.Count),
                    VocabularyPercentage = SafePercentage(test.VocabularyScore ?? 0, vocabularyAnswers.Count),
                    ReadingPercentage = SafePercentage(test.ReadingScore ?? 0, readingAnswers.Count),
                    OverallPercentage = SafePercentage(test.TotalScore ?? 0, testAnswers.Count),
                    CompletedAt = test.CompletedAt ?? DateTime.MinValue,
                    Recommendations = GenerateRecommendations(test.GrammarScore ?? 0, test.VocabularyScore ?? 0, test.ReadingScore ?? 0, test.DeterminedLevel ?? EnglishLevel.A1),
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
    }
}
