using WordWisp.API.Models.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IExerciseRepository
    {
        // CRUD операции
        Task<Exercise> CreateAsync(Exercise exercise);
        Task<Exercise?> GetByIdAsync(int id);
        Task<Exercise?> GetByIdWithQuestionsAsync(int id);
        Task<Exercise?> GetByIdWithQuestionsAndAnswersAsync(int id);
        Task<List<Exercise>> GetByTopicIdAsync(int topicId);
        Task<List<Exercise>> GetActiveByTopicIdAsync(int topicId); // Только активные для студентов
        Task<List<Exercise>> GetByTeacherIdAsync(int teacherId);
        Task<Exercise> UpdateAsync(Exercise exercise);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsOwnerAsync(int exerciseId, int teacherId);

        // Работа с вопросами
        Task<ExerciseQuestion> CreateQuestionAsync(ExerciseQuestion question);
        Task<ExerciseAnswer> CreateAnswerAsync(ExerciseAnswer answer);
        Task<bool> DeleteQuestionAsync(int questionId);
        Task<bool> DeleteAnswerAsync(int answerId);
        Task DeleteQuestionsByExerciseIdAsync(int exerciseId);

        // Работа с попытками студентов
        Task<UserExerciseAttempt> CreateAttemptAsync(UserExerciseAttempt attempt);
        Task<UserExerciseAttempt?> GetAttemptByIdAsync(int attemptId);
        Task<UserExerciseAttempt?> GetAttemptWithAnswersAsync(int attemptId);
        Task<List<UserExerciseAttempt>> GetUserAttemptsAsync(int userId, int exerciseId);
        Task<UserExerciseAttempt> UpdateAttemptAsync(UserExerciseAttempt attempt);

        // Работа с ответами студентов
        Task<UserExerciseAnswer> CreateUserAnswerAsync(UserExerciseAnswer userAnswer);
        Task<List<UserExerciseAnswer>> GetUserAnswersAsync(int attemptId);
    }
}
