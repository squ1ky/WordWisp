using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Models.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class UserExerciseAttemptRepository : IUserExerciseAttemptRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserExerciseAttemptRepository> _logger;

        public UserExerciseAttemptRepository(ApplicationDbContext context, ILogger<UserExerciseAttemptRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserExerciseAttempt> CreateAsync(UserExerciseAttempt attempt)
        {
            attempt.StartedAt = DateTime.UtcNow;
            _context.UserExerciseAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<UserExerciseAttempt?> GetByIdAsync(int id)
        {
            return await _context.UserExerciseAttempts
                .Include(ua => ua.Exercise)
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.Id == id);
        }

        public async Task<UserExerciseAttempt?> GetByIdWithAnswersAsync(int id)
        {
            return await _context.UserExerciseAttempts
                .Include(ua => ua.Exercise)
                    .ThenInclude(e => e.Questions.OrderBy(q => q.Order))
                        .ThenInclude(q => q.Answers.OrderBy(a => a.Order))
                .Include(ua => ua.UserAnswers)
                    .ThenInclude(ans => ans.Question)
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(ua => ua.Id == id);
        }

        public async Task<List<UserExerciseAttempt>> GetByUserIdAsync(int userId)
        {
            return await _context.UserExerciseAttempts
                .Include(ua => ua.Exercise)
                .Where(ua => ua.UserId == userId)
                .OrderByDescending(ua => ua.StartedAt)
                .ToListAsync();
        }

        public async Task<List<UserExerciseAttempt>> GetByExerciseIdAsync(int exerciseId)
        {
            return await _context.UserExerciseAttempts
                .Include(ua => ua.User)
                .Where(ua => ua.ExerciseId == exerciseId)
                .OrderByDescending(ua => ua.StartedAt)
                .ToListAsync();
        }

        public async Task<UserExerciseAttempt?> GetActiveAttemptAsync(int userId, int exerciseId)
        {
            return await _context.UserExerciseAttempts
                .Include(ua => ua.Exercise)
                .FirstOrDefaultAsync(ua => ua.UserId == userId && 
                                         ua.ExerciseId == exerciseId && 
                                         !ua.IsCompleted);
        }

        public async Task<UserExerciseAttempt> UpdateAsync(UserExerciseAttempt attempt)
        {
            _context.UserExerciseAttempts.Update(attempt);
            await _context.SaveChangesAsync();
            return attempt;
        }

        public async Task<int> GetAttemptsCountAsync(int userId, int exerciseId)
        {
            return await _context.UserExerciseAttempts
                .CountAsync(ua => ua.UserId == userId && ua.ExerciseId == exerciseId);
        }

        public async Task<UserExerciseAttempt?> GetBestAttemptAsync(int userId, int exerciseId)
        {
            return await _context.UserExerciseAttempts
                .Where(ua => ua.UserId == userId && ua.ExerciseId == exerciseId && ua.IsCompleted)
                .OrderByDescending(ua => ua.Score)
                .ThenByDescending(ua => ua.CompletedAt)
                .FirstOrDefaultAsync();
        }
    }
}
