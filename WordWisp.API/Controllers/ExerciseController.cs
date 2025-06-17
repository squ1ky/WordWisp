using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordWisp.API.Constants;
using WordWisp.API.Models.Requests.Exercises.Teacher;
using WordWisp.API.Models.Requests.Exercises.Student;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;
        private readonly IUserContextService _userContext;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(
            IExerciseService exerciseService,
            IUserContextService userContext,
            ILogger<ExerciseController> logger)
        {
            _exerciseService = exerciseService;
            _userContext = userContext;
            _logger = logger;
        }

        #region Teacher Endpoints

        /// <summary>
        /// Создание упражнения (только для преподавателей)
        /// </summary>
        [HttpPost("teacher/topics/{topicId}/exercises")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateExercise(int topicId, [FromBody] CreateExerciseRequest request)
        {
            try
            {
                request.TopicId = topicId;
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var exercise = await _exerciseService.CreateExerciseAsync(request, teacherId);
                return Ok(exercise);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating exercise: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение упражнений темы для преподавателя
        /// </summary>
        [HttpGet("teacher/topics/{topicId}/exercises")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetExercisesByTopicForTeacher(int topicId)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var exercises = await _exerciseService.GetExercisesByTopicIdForTeacherAsync(topicId, teacherId);
                return Ok(exercises);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exercises for teacher: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение упражнения с вопросами для преподавателя
        /// </summary>
        [HttpGet("teacher/exercises/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetExerciseForTeacher(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var exercise = await _exerciseService.GetExerciseWithQuestionsForTeacherAsync(id, teacherId);
                
                if (exercise == null)
                    return NotFound(new { message = "Упражнение не найдено" });

                return Ok(exercise);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exercise for teacher: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Обновление упражнения (только для преподавателей)
        /// </summary>
        [HttpPut("teacher/exercises/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateExercise(int id, [FromBody] UpdateExerciseRequest request)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var exercise = await _exerciseService.UpdateExerciseAsync(id, request, teacherId);
                return Ok(exercise);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating exercise: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Удаление упражнения (только для преподавателей)
        /// </summary>
        [HttpDelete("teacher/exercises/{id}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var result = await _exerciseService.DeleteExerciseAsync(id, teacherId);
                
                if (!result)
                    return NotFound(new { message = "Упражнение не найдено" });

                return Ok(new { message = "Упражнение успешно удалено" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting exercise: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Переключение статуса упражнения (активно/неактивно)
        /// </summary>
        [HttpPost("teacher/exercises/{id}/toggle-status")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> ToggleExerciseStatus(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var result = await _exerciseService.ToggleExerciseStatusAsync(id, teacherId);
                
                if (!result)
                    return NotFound(new { message = "Упражнение не найдено" });

                return Ok(new { message = "Статус упражнения изменен" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error toggling exercise status: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение всех упражнений преподавателя
        /// </summary>
        [HttpGet("teacher/exercises")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyExercises()
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var exercises = await _exerciseService.GetMyExercisesAsync(teacherId);
                return Ok(exercises);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting teacher exercises: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение статистики упражнения
        /// </summary>
        [HttpGet("teacher/exercises/{id}/stats")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetExerciseStats(int id)
        {
            try
            {
                var teacherId = _userContext.GetCurrentUserId() ?? 0;
                var stats = await _exerciseService.GetExerciseStatsAsync(id, teacherId);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exercise stats: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        #endregion

        #region Student Endpoints

        /// <summary>
        /// Получение доступных упражнений для студента
        /// </summary>
        [HttpGet("student/topics/{topicId}/exercises")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAvailableExercises(int topicId)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var exercises = await _exerciseService.GetAvailableExercisesAsync(topicId, studentId);
                return Ok(exercises);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting available exercises: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение упражнения для выполнения студентом
        /// </summary>
        [HttpGet("student/exercises/{id}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExerciseForTaking(int id)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var exercise = await _exerciseService.GetExerciseForTakingAsync(id, studentId);
                
                if (exercise == null)
                    return NotFound(new { message = "Упражнение не найдено или недоступно" });

                return Ok(exercise);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exercise for taking: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Начало выполнения упражнения
        /// </summary>
        [HttpPost("student/exercises/{id}/start")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StartExercise(int id)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var attemptId = await _exerciseService.StartExerciseAttemptAsync(id, studentId);
                return Ok(new { attemptId = attemptId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting exercise: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Отправка ответов на упражнение
        /// </summary>
        [HttpPost("student/exercises/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitExercise([FromBody] SubmitExerciseRequest request)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var result = await _exerciseService.SubmitExerciseAsync(request, studentId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting exercise: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение результатов упражнения для студента
        /// </summary>
        [HttpGet("student/exercises/{id}/results")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetExerciseResults(int id)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var results = await _exerciseService.GetExerciseResultsAsync(id, studentId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting exercise results: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение попыток студента по упражнению
        /// </summary>
        [HttpGet("student/exercises/{id}/attempts")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetUserAttempts(int id)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var attempts = await _exerciseService.GetUserAttemptsAsync(id, studentId);
                return Ok(attempts);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user attempts: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Получение деталей конкретной попытки
        /// </summary>
        [HttpGet("student/attempts/{attemptId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAttemptDetails(int attemptId)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var details = await _exerciseService.GetAttemptDetailsAsync(attemptId, studentId);
                return Ok(details);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting attempt details: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        /// <summary>
        /// Проверка возможности начать новую попытку
        /// </summary>
        [HttpGet("student/exercises/{id}/can-start")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CanStartAttempt(int id)
        {
            try
            {
                var studentId = _userContext.GetCurrentUserId() ?? 0;
                var canStart = await _exerciseService.CanUserStartAttemptAsync(id, studentId);
                return Ok(new { canStart = canStart });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking can start attempt: {ex.Message}");
                return StatusCode(500, new { message = ErrorMessages.InternalServerError });
            }
        }

        #endregion
    }
}
