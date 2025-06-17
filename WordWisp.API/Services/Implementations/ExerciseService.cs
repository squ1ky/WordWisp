using WordWisp.API.Models.DTOs.Exercises;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Exercises.Teacher;
using WordWisp.API.Models.Requests.Exercises.Student;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly ILogger<ExerciseService> _logger;

        public ExerciseService(
            IExerciseRepository exerciseRepository,
            ITopicRepository topicRepository,
            IMaterialRepository materialRepository,
            ILogger<ExerciseService> logger)
        {
            _exerciseRepository = exerciseRepository;
            _topicRepository = topicRepository;
            _materialRepository = materialRepository;
            _logger = logger;
        }

        #region Teacher Methods

        public async Task<ExerciseDto> CreateExerciseAsync(CreateExerciseRequest request, int teacherId)
        {
            // Проверяем права на топик
            var topic = await _topicRepository.GetByIdAsync(request.TopicId);
            if (topic == null)
                throw new ArgumentException("Топик не найден");

            if (topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет прав на создание упражнений в этом топике");

            // Проверяем материал (если указан)
            if (request.MaterialId.HasValue)
            {
                var material = await _materialRepository.GetByIdAsync(request.MaterialId.Value);
                if (material == null || material.TopicId != request.TopicId)
                    throw new ArgumentException("Материал не найден или не принадлежит указанному топику");
            }

            var exercise = new Exercise
            {
                Title = request.Title,
                Description = request.Description,
                ExerciseType = request.ExerciseType,
                TopicId = request.TopicId,
                MaterialId = request.MaterialId,
                TimeLimit = request.TimeLimit,
                MaxAttempts = request.MaxAttempts,
                PassingScore = request.PassingScore,
                IsActive = request.IsActive,
                Order = request.Order,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdExercise = await _exerciseRepository.CreateAsync(exercise);

            // Создаем вопросы
            foreach (var questionRequest in request.Questions)
            {
                var question = new ExerciseQuestion
                {
                    Question = questionRequest.Question,
                    QuestionImagePath = questionRequest.QuestionImagePath,
                    QuestionAudioPath = questionRequest.QuestionAudioPath,
                    Order = questionRequest.Order,
                    Points = questionRequest.Points,
                    ExerciseId = createdExercise.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var createdQuestion = await _exerciseRepository.CreateQuestionAsync(question);

                // Создаем ответы
                foreach (var answerRequest in questionRequest.Answers)
                {
                    var answer = new ExerciseAnswer
                    {
                        AnswerText = answerRequest.AnswerText,
                        AnswerImagePath = answerRequest.AnswerImagePath,
                        IsCorrect = answerRequest.IsCorrect,
                        Order = answerRequest.Order,
                        QuestionId = createdQuestion.Id
                    };

                    await _exerciseRepository.CreateAnswerAsync(answer);
                }
            }

            return await GetExerciseWithQuestionsForTeacherAsync(createdExercise.Id, teacherId) 
                ?? throw new Exception("Ошибка при получении созданного упражнения");
        }

        public async Task<ExerciseDto?> GetExerciseByIdForTeacherAsync(int id, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(id);
            if (exercise == null) return null;

            // Проверяем права доступа
            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет доступа к этому упражнению");

            return MapToDto(exercise);
        }

        public async Task<ExerciseDto?> GetExerciseWithQuestionsForTeacherAsync(int id, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdWithQuestionsAndAnswersAsync(id);
            if (exercise == null) return null;

            // Проверяем права доступа
            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет доступа к этому упражнению");

            return MapToDtoWithQuestions(exercise);
        }

        public async Task<List<ExerciseDto>> GetExercisesByTopicIdForTeacherAsync(int topicId, int teacherId)
        {
            // Проверяем права на топик
            var topic = await _topicRepository.GetByIdAsync(topicId);
            if (topic == null || topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет доступа к этому топику");

            var exercises = await _exerciseRepository.GetByTopicIdAsync(topicId);
            return exercises.Select(MapToDto).ToList();
        }

        public async Task<List<ExerciseDto>> GetMyExercisesAsync(int teacherId)
        {
            var exercises = await _exerciseRepository.GetByTeacherIdAsync(teacherId);
            return exercises.Select(MapToDto).ToList();
        }

        public async Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseRequest request, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdWithQuestionsAndAnswersAsync(id);
            if (exercise == null)
                throw new ArgumentException("Упражнение не найдено");

            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет прав на редактирование этого упражнения");

            // Проверяем материал (если указан)
            if (request.MaterialId.HasValue)
            {
                var material = await _materialRepository.GetByIdAsync(request.MaterialId.Value);
                if (material == null || material.TopicId != exercise.TopicId)
                    throw new ArgumentException("Материал не найден или не принадлежит топику упражнения");
            }

            // Обновляем основные поля
            exercise.Title = request.Title;
            exercise.Description = request.Description;
            exercise.ExerciseType = request.ExerciseType;
            exercise.MaterialId = request.MaterialId;
            exercise.TimeLimit = request.TimeLimit;
            exercise.MaxAttempts = request.MaxAttempts;
            exercise.PassingScore = request.PassingScore;
            exercise.IsActive = request.IsActive;
            exercise.Order = request.Order;
            exercise.UpdatedAt = DateTime.UtcNow;

            await _exerciseRepository.UpdateAsync(exercise);

            // Удаляем старые вопросы
            await _exerciseRepository.DeleteQuestionsByExerciseIdAsync(exercise.Id);

            // Создаем новые вопросы
            foreach (var questionRequest in request.Questions)
            {
                var question = new ExerciseQuestion
                {
                    Question = questionRequest.Question,
                    QuestionImagePath = questionRequest.QuestionImagePath,
                    QuestionAudioPath = questionRequest.QuestionAudioPath,
                    Order = questionRequest.Order,
                    Points = questionRequest.Points,
                    ExerciseId = exercise.Id,
                    CreatedAt = DateTime.UtcNow
                };

                var createdQuestion = await _exerciseRepository.CreateQuestionAsync(question);

                foreach (var answerRequest in questionRequest.Answers)
                {
                    var answer = new ExerciseAnswer
                    {
                        AnswerText = answerRequest.AnswerText,
                        AnswerImagePath = answerRequest.AnswerImagePath,
                        IsCorrect = answerRequest.IsCorrect,
                        Order = answerRequest.Order,
                        QuestionId = createdQuestion.Id
                    };

                    await _exerciseRepository.CreateAnswerAsync(answer);
                }
            }

            return await GetExerciseWithQuestionsForTeacherAsync(id, teacherId) 
                ?? throw new Exception("Ошибка при получении обновленного упражнения");
        }

        public async Task<bool> DeleteExerciseAsync(int id, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(id);
            if (exercise == null) return false;

            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет прав на удаление этого упражнения");

            return await _exerciseRepository.DeleteAsync(id);
        }

        public async Task<bool> ToggleExerciseStatusAsync(int id, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(id);
            if (exercise == null) return false;

            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет прав на изменение этого упражнения");

            exercise.IsActive = !exercise.IsActive;
            exercise.UpdatedAt = DateTime.UtcNow;

            await _exerciseRepository.UpdateAsync(exercise);
            return true;
        }

        public async Task<object> GetExerciseStatsAsync(int exerciseId, int teacherId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null)
                throw new ArgumentException("Упражнение не найдено");

            if (exercise.Topic.CreatedBy != teacherId)
                throw new UnauthorizedAccessException("У вас нет доступа к статистике этого упражнения");

            // TODO: Реализовать получение реальной статистики
            return new
            {
                exerciseId = exerciseId,
                totalAttempts = 0,
                completedAttempts = 0,
                passedAttempts = 0,
                averageScore = 0.0,
                bestScore = 0.0,
                lastAttemptDate = (DateTime?)null
            };
        }

        #endregion

        #region Student Methods

        public async Task<List<StudentExerciseDto>> GetAvailableExercisesAsync(int topicId, int studentId)
        {
            var exercises = await _exerciseRepository.GetActiveByTopicIdAsync(topicId);
            return exercises.Select(MapToStudentDto).ToList();
        }

        public async Task<StudentExerciseDto?> GetExerciseForTakingAsync(int exerciseId, int studentId)
        {
            var exercise = await _exerciseRepository.GetByIdWithQuestionsAndAnswersAsync(exerciseId);
            if (exercise == null || !exercise.IsActive) return null;

            return MapToStudentDtoWithQuestions(exercise);
        }

        public async Task<int> StartExerciseAttemptAsync(int exerciseId, int studentId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null)
                throw new ArgumentException("Упражнение не найдено");

            if (!exercise.IsActive)
                throw new ArgumentException("Упражнение неактивно");

            // Проверяем количество попыток
            var attempts = await _exerciseRepository.GetUserAttemptsAsync(studentId, exerciseId);
            if (attempts.Count >= exercise.MaxAttempts)
                throw new ArgumentException("Превышено максимальное количество попыток");

            var attempt = new UserExerciseAttempt
            {
                UserId = studentId,
                ExerciseId = exerciseId,
                StartedAt = DateTime.UtcNow,
                MaxPossibleScore = 100,
                Score = 0,
                IsCompleted = false,
                IsPassed = false,
                TimeSpentSeconds = 0
            };

            var createdAttempt = await _exerciseRepository.CreateAttemptAsync(attempt);
            return createdAttempt.Id;
        }

        public async Task<object> SubmitExerciseAsync(SubmitExerciseRequest request, int studentId)
        {
            var attempt = await _exerciseRepository.GetAttemptWithAnswersAsync(request.AttemptId);
            if (attempt == null)
                throw new ArgumentException("Попытка не найдена");

            if (attempt.UserId != studentId)
                throw new UnauthorizedAccessException("У вас нет доступа к этой попытке");

            if (attempt.IsCompleted)
                throw new ArgumentException("Попытка уже завершена");

            // Подсчитываем результаты
            decimal totalPoints = 0;
            decimal earnedPoints = 0;

            foreach (var question in attempt.Exercise.Questions)
            {
                totalPoints += question.Points;
                
                var userAnswer = request.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                if (userAnswer != null)
                {
                    bool isCorrect = false;
                    decimal pointsEarned = 0;

                    // Проверяем правильность ответа
                    if (userAnswer.SelectedAnswerIds.Any())
                    {
                        var correctAnswers = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();
                        isCorrect = userAnswer.SelectedAnswerIds.OrderBy(x => x).SequenceEqual(correctAnswers.OrderBy(x => x));
                    }
                    else if (!string.IsNullOrEmpty(userAnswer.TextAnswer))
                    {
                        // Для текстовых ответов - упрощенная проверка
                        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
                        isCorrect = correctAnswer != null && 
                                   string.Equals(userAnswer.TextAnswer.Trim(), correctAnswer.AnswerText.Trim(), 
                                                StringComparison.OrdinalIgnoreCase);
                    }

                    if (isCorrect)
                    {
                        pointsEarned = question.Points;
                        earnedPoints += pointsEarned;
                    }

                    // Сохраняем ответ пользователя
                    var userExerciseAnswer = new UserExerciseAnswer
                    {
                        UserAttemptId = attempt.Id,
                        QuestionId = question.Id,
                        AnswerText = userAnswer.TextAnswer,
                        SelectedAnswerId = userAnswer.SelectedAnswerIds.FirstOrDefault(),
                        IsCorrect = isCorrect,
                        PointsEarned = pointsEarned,
                        AnsweredAt = DateTime.UtcNow
                    };

                    await _exerciseRepository.CreateUserAnswerAsync(userExerciseAnswer);
                }
            }

            // Обновляем попытку
            attempt.Score = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
            attempt.IsCompleted = true;
            attempt.IsPassed = attempt.Score >= attempt.Exercise.PassingScore;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.TimeSpentSeconds = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;

            await _exerciseRepository.UpdateAttemptAsync(attempt);

            return new
            {
                attemptId = attempt.Id,
                score = attempt.Score,
                isPassed = attempt.IsPassed,
                timeSpent = attempt.TimeSpentSeconds,
                earnedPoints = earnedPoints,
                totalPoints = totalPoints,
                passingScore = attempt.Exercise.PassingScore
            };
        }

        public async Task<object> GetExerciseResultsAsync(int exerciseId, int studentId)
        {
            var attempts = await _exerciseRepository.GetUserAttemptsAsync(studentId, exerciseId);
            
            return new
            {
                exerciseId = exerciseId,
                attempts = attempts.Select(a => new
                {
                    id = a.Id,
                    score = a.Score,
                    isPassed = a.IsPassed,
                    startedAt = a.StartedAt,
                    completedAt = a.CompletedAt,
                    timeSpent = a.TimeSpentSeconds,
                    isCompleted = a.IsCompleted
                }).ToList(),
                bestScore = attempts.Any() ? attempts.Max(a => a.Score) : 0,
                isPassed = attempts.Any(a => a.IsPassed),
                attemptsUsed = attempts.Count
            };
        }

        public async Task<List<object>> GetUserAttemptsAsync(int exerciseId, int studentId)
        {
            var attempts = await _exerciseRepository.GetUserAttemptsAsync(studentId, exerciseId);
            
            return attempts.Select(a => new
            {
                id = a.Id,
                score = a.Score,
                isPassed = a.IsPassed,
                startedAt = a.StartedAt,
                completedAt = a.CompletedAt,
                timeSpent = a.TimeSpentSeconds,
                isCompleted = a.IsCompleted
            }).Cast<object>().ToList();
        }

        public async Task<object> GetAttemptDetailsAsync(int attemptId, int studentId)
        {
            var attempt = await _exerciseRepository.GetAttemptWithAnswersAsync(attemptId);
            if (attempt == null)
                throw new ArgumentException("Попытка не найдена");

            if (attempt.UserId != studentId)
                throw new UnauthorizedAccessException("У вас нет доступа к этой попытке");

            return new
            {
                id = attempt.Id,
                exerciseTitle = attempt.Exercise.Title,
                score = attempt.Score,
                isPassed = attempt.IsPassed,
                startedAt = attempt.StartedAt,
                completedAt = attempt.CompletedAt,
                timeSpent = attempt.TimeSpentSeconds,
                userAnswers = attempt.UserAnswers.Select(ua => new
                {
                    questionId = ua.QuestionId,
                    answerText = ua.AnswerText,
                    selectedAnswerId = ua.SelectedAnswerId,
                    isCorrect = ua.IsCorrect,
                    pointsEarned = ua.PointsEarned
                }).ToList()
            };
        }

        public async Task<bool> CanUserAccessExerciseAsync(int exerciseId, int studentId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            return exercise != null && exercise.IsActive;
        }

        public async Task<bool> CanUserStartAttemptAsync(int exerciseId, int studentId)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(exerciseId);
            if (exercise == null || !exercise.IsActive) return false;

            var attempts = await _exerciseRepository.GetUserAttemptsAsync(studentId, exerciseId);
            return attempts.Count < exercise.MaxAttempts;
        }

        #endregion

        #region Mapping Methods

        private ExerciseDto MapToDto(Exercise exercise)
        {
            return new ExerciseDto
            {
                Id = exercise.Id,
                Title = exercise.Title,
                Description = exercise.Description,
                ExerciseType = exercise.ExerciseType,
                TopicId = exercise.TopicId,
                TopicTitle = exercise.Topic?.Title ?? "",
                MaterialId = exercise.MaterialId,
                MaterialTitle = exercise.Material?.Title,
                TimeLimit = exercise.TimeLimit,
                MaxAttempts = exercise.MaxAttempts,
                PassingScore = exercise.PassingScore,
                IsActive = exercise.IsActive,
                Order = exercise.Order,
                CreatedAt = exercise.CreatedAt,
                UpdatedAt = exercise.UpdatedAt
            };
        }

        private ExerciseDto MapToDtoWithQuestions(Exercise exercise)
        {
            var dto = MapToDto(exercise);
            dto.Questions = exercise.Questions?.OrderBy(q => q.Order).Select(q => new ExerciseQuestionDto
            {
                Id = q.Id,
                Question = q.Question,
                QuestionImagePath = q.QuestionImagePath,
                QuestionAudioPath = q.QuestionAudioPath,
                Order = q.Order,
                Points = q.Points,
                Answers = q.Answers?.OrderBy(a => a.Order).Select(a => new ExerciseAnswerDto
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText,
                    AnswerImagePath = a.AnswerImagePath,
                    IsCorrect = a.IsCorrect,
                    Order = a.Order
                }).ToList() ?? new List<ExerciseAnswerDto>()
            }).ToList() ?? new List<ExerciseQuestionDto>();

            return dto;
        }

        private StudentExerciseDto MapToStudentDto(Exercise exercise)
        {
            return new StudentExerciseDto
            {
                Id = exercise.Id,
                Title = exercise.Title,
                Description = exercise.Description,
                ExerciseType = exercise.ExerciseType,
                TopicId = exercise.TopicId,
                TopicTitle = exercise.Topic?.Title ?? "",
                TimeLimit = exercise.TimeLimit,
                MaxAttempts = exercise.MaxAttempts,
                PassingScore = exercise.PassingScore
            };
        }

        private StudentExerciseDto MapToStudentDtoWithQuestions(Exercise exercise)
        {
            var dto = MapToStudentDto(exercise);
            dto.Questions = exercise.Questions?.OrderBy(q => q.Order).Select(q => new StudentExerciseQuestionDto
            {
                Id = q.Id,
                Question = q.Question,
                QuestionImagePath = q.QuestionImagePath,
                QuestionAudioPath = q.QuestionAudioPath,
                Order = q.Order,
                Points = q.Points,
                Answers = q.Answers?.OrderBy(a => a.Order).Select(a => new StudentExerciseAnswerDto
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText,
                    AnswerImagePath = a.AnswerImagePath,
                    Order = a.Order
                    // НЕ включаем IsCorrect для студентов
                }).ToList() ?? new List<StudentExerciseAnswerDto>()
            }).ToList() ?? new List<StudentExerciseQuestionDto>();

            return dto;
        }

        #endregion
    }
}
