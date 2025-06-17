using WordWisp.API.Models.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IUserExerciseAttemptRepository
    {
        Task<UserExerciseAttempt> CreateAsync(UserExerciseAttempt attempt);
        Task<UserExerciseAttempt?> GetByIdAsync(int id);
        Task<UserExerciseAttempt?> GetByIdWithAnswersAsync(int id);
        Task<List<UserExerciseAttempt>> GetByUserIdAsync(int userId);
        Task<List<UserExerciseAttempt>> GetByExerciseIdAsync(int exerciseId);
        Task<UserExerciseAttempt?> GetActiveAttemptAsync(int userId, int exerciseId);
        Task<UserExerciseAttempt> UpdateAsync(UserExerciseAttempt attempt);
        Task<int> GetAttemptsCountAsync(int userId, int exerciseId);
        Task<UserExerciseAttempt?> GetBestAttemptAsync(int userId, int exerciseId);
    }
}
