using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Exercises;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Student.Exercises
{
    public class ExerciseResultsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ExerciseResultsModel> _logger;

        public ExerciseResultsModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<ExerciseResultsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public StudentExerciseDto? Exercise { get; set; }
        public ExerciseResultsDto? Results { get; set; }
        public AttemptDetailsDto? CurrentAttempt { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id, int? attemptId = null)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Student")
                return RedirectToPage("/Auth/Login");

            try
            {
                await LoadExerciseAsync(id, token);
                await LoadResultsAsync(id, token);
                
                if (attemptId.HasValue)
                {
                    await LoadAttemptDetailsAsync(attemptId.Value, token);
                }

                if (Exercise == null)
                {
                    ErrorMessage = "Упражнение не найдено";
                    return Page();
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading exercise results: {ex.Message}");
                ErrorMessage = $"Ошибка загрузки результатов: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadExerciseAsync(int exerciseId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/student/exercises/{exerciseId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Exercise = JsonSerializer.Deserialize<StudentExerciseDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading exercise: {ex.Message}");
            }
        }

        private async Task LoadResultsAsync(int exerciseId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/student/exercises/{exerciseId}/results");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var resultsElement = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    Results = new ExerciseResultsDto
                    {
                        ExerciseId = exerciseId,
                        BestScore = resultsElement.TryGetProperty("bestScore", out var bestScore) ? bestScore.GetDecimal() : 0,
                        IsPassed = resultsElement.TryGetProperty("isPassed", out var isPassed) && isPassed.GetBoolean(),
                        AttemptsUsed = resultsElement.TryGetProperty("attemptsUsed", out var attempts) ? attempts.GetInt32() : 0,
                        Attempts = new List<AttemptSummaryDto>()
                    };

                    if (resultsElement.TryGetProperty("attempts", out var attemptsArray))
                    {
                        foreach (var attemptElement in attemptsArray.EnumerateArray())
                        {
                            Results.Attempts.Add(new AttemptSummaryDto
                            {
                                Id = attemptElement.GetProperty("id").GetInt32(),
                                Score = attemptElement.GetProperty("score").GetDecimal(),
                                IsPassed = attemptElement.GetProperty("isPassed").GetBoolean(),
                                StartedAt = attemptElement.GetProperty("startedAt").GetDateTime(),
                                CompletedAt = attemptElement.TryGetProperty("completedAt", out var completed) 
                                    ? completed.GetDateTime() : null,
                                TimeSpent = attemptElement.GetProperty("timeSpent").GetInt32(),
                                IsCompleted = attemptElement.GetProperty("isCompleted").GetBoolean()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading results: {ex.Message}");
            }
        }

        private async Task LoadAttemptDetailsAsync(int attemptId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/student/attempts/{attemptId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var attemptElement = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    CurrentAttempt = new AttemptDetailsDto
                    {
                        Id = attemptElement.GetProperty("id").GetInt32(),
                        ExerciseTitle = attemptElement.GetProperty("exerciseTitle").GetString() ?? "",
                        Score = attemptElement.GetProperty("score").GetDecimal(),
                        IsPassed = attemptElement.GetProperty("isPassed").GetBoolean(),
                        StartedAt = attemptElement.GetProperty("startedAt").GetDateTime(),
                        CompletedAt = attemptElement.TryGetProperty("completedAt", out var completed) 
                            ? completed.GetDateTime() : null,
                        TimeSpent = attemptElement.GetProperty("timeSpent").GetInt32(),
                        UserAnswers = new List<UserAnswerDto>()
                    };

                    if (attemptElement.TryGetProperty("userAnswers", out var answersArray))
                    {
                        foreach (var answerElement in answersArray.EnumerateArray())
                        {
                            CurrentAttempt.UserAnswers.Add(new UserAnswerDto
                            {
                                QuestionId = answerElement.GetProperty("questionId").GetInt32(),
                                AnswerText = answerElement.TryGetProperty("answerText", out var text) ? text.GetString() : null,
                                SelectedAnswerId = answerElement.TryGetProperty("selectedAnswerId", out var selected) ? selected.GetInt32() : null,
                                IsCorrect = answerElement.TryGetProperty("isCorrect", out var correct) ? correct.GetBoolean() : null,
                                PointsEarned = answerElement.GetProperty("pointsEarned").GetDecimal()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading attempt details: {ex.Message}");
            }
        }
    }

    // DTO классы для результатов
    public class ExerciseResultsDto
    {
        public int ExerciseId { get; set; }
        public decimal BestScore { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptsUsed { get; set; }
        public List<AttemptSummaryDto> Attempts { get; set; } = new();
    }

    public class AttemptSummaryDto
    {
        public int Id { get; set; }
        public decimal Score { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeSpent { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class AttemptDetailsDto
    {
        public int Id { get; set; }
        public string ExerciseTitle { get; set; } = "";
        public decimal Score { get; set; }
        public bool IsPassed { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeSpent { get; set; }
        public List<UserAnswerDto> UserAnswers { get; set; } = new();
    }

    public class UserAnswerDto
    {
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public int? SelectedAnswerId { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal PointsEarned { get; set; }
    }
}
