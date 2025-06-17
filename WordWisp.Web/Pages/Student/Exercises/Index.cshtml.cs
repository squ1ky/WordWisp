using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Exercises;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Student.Exercises
{
    public class StudentExercisesIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StudentExercisesIndexModel> _logger;

        public StudentExercisesIndexModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<StudentExercisesIndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicDto? Topic { get; set; }
        public List<StudentExerciseDto> Exercises { get; set; } = new();
        public Dictionary<int, object> UserProgress { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int topicId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Student")
                return RedirectToPage("/Auth/Login");

            try
            {
                await LoadTopicExercisesAsync(topicId, token);
                
                if (Topic == null)
                {
                    ErrorMessage = "Тема не найдена";
                    return Page();
                }

                await LoadUserProgressAsync(token);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading student exercises: {ex.Message}");
                ErrorMessage = "Ошибка загрузки упражнений";
                return Page();
            }
        }

        private async Task LoadTopicExercisesAsync(int topicId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Загружаем информацию о теме
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/public/{topicId}");
                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                // Загружаем доступные упражнения
                var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/student/topics/{topicId}/exercises");
                if (exercisesResponse.IsSuccessStatusCode)
                {
                    var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                    Exercises = JsonSerializer.Deserialize<List<StudentExerciseDto>>(exercisesJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<StudentExerciseDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading topic exercises: {ex.Message}");
            }
        }

        private async Task LoadUserProgressAsync(string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                foreach (var exercise in Exercises)
                {
                    try
                    {
                        var response = await httpClient.GetAsync($"{apiBaseUrl}/api/student/exercises/{exercise.Id}/results");
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var results = JsonSerializer.Deserialize<JsonElement>(json);
                            
                            UserProgress[exercise.Id] = new
                            {
                                BestScore = results.TryGetProperty("bestScore", out var bestScore) ? bestScore.GetDecimal() : 0,
                                IsPassed = results.TryGetProperty("isPassed", out var isPassed) && isPassed.GetBoolean(),
                                AttemptsUsed = results.TryGetProperty("attemptsUsed", out var attempts) ? attempts.GetInt32() : 0
                            };
                        }
                        else
                        {
                            UserProgress[exercise.Id] = new
                            {
                                BestScore = 0m,
                                IsPassed = false,
                                AttemptsUsed = 0
                            };
                        }
                    }
                    catch
                    {
                        UserProgress[exercise.Id] = new
                        {
                            BestScore = 0m,
                            IsPassed = false,
                            AttemptsUsed = 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading user progress: {ex.Message}");
            }
        }
    }
}
