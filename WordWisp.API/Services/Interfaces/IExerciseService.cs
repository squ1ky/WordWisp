using WordWisp.API.Models.DTOs.Exercises;
using WordWisp.API.Models.Requests.Exercises.Teacher;
using WordWisp.API.Models.Requests.Exercises.Student;

namespace WordWisp.API.Services.Interfaces
{
    public interface IExerciseService
    {
        #region Teacher Methods
        
        // CRUD операции для преподавателей
        Task<ExerciseDto> CreateExerciseAsync(CreateExerciseRequest request, int teacherId);
        Task<ExerciseDto?> GetExerciseByIdForTeacherAsync(int id, int teacherId);
        Task<ExerciseDto?> GetExerciseWithQuestionsForTeacherAsync(int id, int teacherId);
        Task<List<ExerciseDto>> GetExercisesByTopicIdForTeacherAsync(int topicId, int teacherId);
        Task<List<ExerciseDto>> GetMyExercisesAsync(int teacherId);
        Task<ExerciseDto> UpdateExerciseAsync(int id, UpdateExerciseRequest request, int teacherId);
        Task<bool> DeleteExerciseAsync(int id, int teacherId);
        
        // Управление статусом для преподавателей
        Task<bool> ToggleExerciseStatusAsync(int id, int teacherId);
        
        // Статистика для преподавателей
        Task<object> GetExerciseStatsAsync(int exerciseId, int teacherId);
        
        #endregion

        #region Student Methods
        
        // Просмотр упражнений для студентов
        Task<List<StudentExerciseDto>> GetAvailableExercisesAsync(int topicId, int studentId);
        Task<StudentExerciseDto?> GetExerciseForTakingAsync(int exerciseId, int studentId);
        
        // Выполнение упражнений студентами
        Task<int> StartExerciseAttemptAsync(int exerciseId, int studentId);
        Task<object> SubmitExerciseAsync(SubmitExerciseRequest request, int studentId);
        
        // Результаты для студентов
        Task<object> GetExerciseResultsAsync(int exerciseId, int studentId);
        Task<List<object>> GetUserAttemptsAsync(int exerciseId, int studentId);
        Task<object> GetAttemptDetailsAsync(int attemptId, int studentId);
        
        // Проверка доступа для студентов
        Task<bool> CanUserAccessExerciseAsync(int exerciseId, int studentId);
        Task<bool> CanUserStartAttemptAsync(int exerciseId, int studentId);
        
        #endregion
    }
}
