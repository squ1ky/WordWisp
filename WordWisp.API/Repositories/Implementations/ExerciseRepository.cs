using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Models.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExerciseRepository> _logger;

        public ExerciseRepository(ApplicationDbContext context, ILogger<ExerciseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region CRUD Operations

        public async Task<Exercise> CreateAsync(Exercise exercise)
        {
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises
                .Include(e => e.Topic)
                .Include(e => e.Material)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Exercise?> GetByIdWithQuestionsAsync(int id)
        {
            return await _context.Exercises
                .Include(e => e.Topic)
                .Include(e => e.Material)
                .Include(e => e.Questions.OrderBy(q => q.Order))
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Exercise?> GetByIdWithQuestionsAndAnswersAsync(int id)
        {
            return await _context.Exercises
                .Include(e => e.Topic)
                .Include(e => e.Material)
                .Include(e => e.Questions.OrderBy(q => q.Order))
                    .ThenInclude(q => q.Answers.OrderBy(a => a.Order))
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Exercise>> GetByTopicIdAsync(int topicId)
        {
            return await _context.Exercises
                .Include(e => e.Material)
                .Where(e => e.TopicId == topicId)
                .OrderBy(e => e.Order)
                .ThenBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Exercise>> GetActiveByTopicIdAsync(int topicId)
        {
            return await _context.Exercises
                .Include(e => e.Material)
                .Where(e => e.TopicId == topicId && e.IsActive)
                .OrderBy(e => e.Order)
                .ThenBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Exercise>> GetByTeacherIdAsync(int teacherId)
        {
            return await _context.Exercises
                .Include(e => e.Topic)
                .Include(e => e.Material)
                .Where(e => e.Topic.CreatedBy == teacherId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<Exercise> UpdateAsync(Exercise exercise)
        {
            var existingExercise = await _context.Exercises.FindAsync(exercise.Id);
            if (existingExercise == null)
                throw new ArgumentException("Упражнение не найдено");

            // Обновляем свойства
            existingExercise.Title = exercise.Title;
            existingExercise.Description = exercise.Description;
            existingExercise.ExerciseType = exercise.ExerciseType;
            existingExercise.MaterialId = exercise.MaterialId;
            existingExercise.TimeLimit = exercise.TimeLimit;
            existingExercise.MaxAttempts = exercise.MaxAttempts;
            existingExercise.PassingScore = exercise.PassingScore;
            existingExercise.IsActive = exercise.IsActive;
            existingExercise.Order = exercise.Order;
            existingExercise.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingExercise;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return false;

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == id);
        }

        public async Task<bool> IsOwnerAsync(int exerciseId, int teacherId)
        {
            return await _context.Exercises
                .AnyAsync(e => e.Id == exerciseId && e.Topic.CreatedBy == teacherId);
        }

        #endregion

        #region Questions Management

        public async Task<ExerciseQuestion> CreateQuestionAsync(ExerciseQuestion question)
        {
            _context.ExerciseQuestions.Add(question);
            await _context.SaveChangesAsync();
            return question;
        }

        public async Task<ExerciseAnswer> CreateAnswerAsync(ExerciseAnswer answer)
        {
            _context.ExerciseAnswers.Add(answer);
            await _context.SaveChangesAsync();
            return answer;
        }

        public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            var question = await _context.ExerciseQuestions.FindAsync(questionId);
            if (question == null) return false;

            _context.ExerciseQuestions.Remove(question);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAnswerAsync(int answerId)
        {
            var answer = await _context.ExerciseAnswers.FindAsync(answerId);
            if (answer == null) return false;

            _context.ExerciseAnswers.Remove(answer);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeleteQuestionsByExerciseIdAsync(int exerciseId)
        {
            var questions = await _context.ExerciseQuestions
                .Where(q => q.ExerciseId == exerciseId)
                .ToListAsync();

            _context.ExerciseQuestions.RemoveRange(questions);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Student Attempts

        public async Task<UserExerciseAttempt> CreateAttemptAsync(UserExerciseAttempt attempt)
        {
            _context.UserExerciseAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<UserExerciseAttempt?> GetAttemptByIdAsync(int attemptId)
        {
            return await _context.UserExerciseAttempts
                .Include(a => a.Exercise)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
        }

        public async Task<UserExerciseAttempt?> GetAttemptWithAnswersAsync(int attemptId)
        {
            return await _context.UserExerciseAttempts
                .Include(a => a.Exercise)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Answers)
                .Include(a => a.UserAnswers)
                .FirstOrDefaultAsync(a => a.Id == attemptId);
        }

        public async Task<List<UserExerciseAttempt>> GetUserAttemptsAsync(int userId, int exerciseId)
        {
            return await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId && a.ExerciseId == exerciseId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();
        }

        public async Task<UserExerciseAttempt> UpdateAttemptAsync(UserExerciseAttempt attempt)
        {
            _context.UserExerciseAttempts.Update(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<UserExerciseAnswer> CreateUserAnswerAsync(UserExerciseAnswer userAnswer)
        {
            _context.UserExerciseAnswers.Add(userAnswer);
            await _context.SaveChangesAsync();
            return userAnswer;
        }

        public async Task<List<UserExerciseAnswer>> GetUserAnswersAsync(int attemptId)
        {
            return await _context.UserExerciseAnswers
                .Where(a => a.UserAttemptId == attemptId)
                .ToListAsync();
        }

        #endregion
    }
}
