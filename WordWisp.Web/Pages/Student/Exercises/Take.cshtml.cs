using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Exercises;
using WordWisp.Web.Models.Exercises;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Student.Exercises
{
    public class TakeExerciseModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TakeExerciseModel> _logger;

        public TakeExerciseModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<TakeExerciseModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public TakeExerciseInputModel Input { get; set; } = new();

        public StudentExerciseDto? Exercise { get; set; }
        public int AttemptId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanStart { get; set; } = true;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            _logger.LogInformation($"TakeExercise OnGetAsync called with exercise id: {id}");
            
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Student")
                return RedirectToPage("/Auth/Login");

            try
            {
                // ИСПРАВЛЕНО: Загружаем упражнение с правильным ID
                _logger.LogInformation($"Loading exercise with ID: {id}");
                await LoadExerciseAsync(id, token);
                
                if (Exercise == null)
                {
                    _logger.LogError($"Exercise {id} not found");
                    ErrorMessage = "Упражнение не найдено или недоступно";
                    return Page();
                }

                _logger.LogInformation($"Exercise loaded: {Exercise.Title}, TopicId: {Exercise.TopicId}");

                // Проверяем, можно ли начать новую попытку
                await CheckCanStartAsync(id, token);
                
                if (!CanStart)
                {
                    _logger.LogWarning($"Cannot start exercise {id}");
                    // ИСПРАВЛЕНО: Используем Exercise.TopicId для редиректа
                    return RedirectToPage("/Student/Exercises/Index", new { topicId = Exercise.TopicId });
                }

                // Начинаем новую попытку
                await StartAttemptAsync(id, token);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading take exercise page: {ex.Message}");
                ErrorMessage = $"Ошибка загрузки упражнения: {ex.Message}";
                return Page();
            }
        }


        public async Task<IActionResult> OnPostAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    exerciseId = Input.ExerciseId,
                    attemptId = Input.AttemptId,
                    answers = Input.Answers.Select(a => new
                    {
                        questionId = a.QuestionId,
                        selectedAnswerIds = a.SelectedAnswerIds,
                        textAnswer = a.TextAnswer
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/student/exercises/submit", content);

                if (response.IsSuccessStatusCode)
                {
                    var resultJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(resultJson);
                    
                    var attemptId = result.GetProperty("attemptId").GetInt32();
                    return RedirectToPage("/Student/Exercises/Results", new { id, attemptId });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                    ErrorMessage = errorData.TryGetProperty("message", out var msg) 
                        ? msg.GetString() 
                        : "Ошибка отправки ответов";
                    
                    await LoadExerciseAsync(id, token);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error submitting exercise: {ex.Message}");
                ErrorMessage = "Не удалось отправить ответы";
                await LoadExerciseAsync(id, token);
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

                // ИСПРАВЛЕНО: Используем правильный exerciseId
                var requestUrl = $"{apiBaseUrl}/api/student/exercises/{exerciseId}";
                _logger.LogInformation($"Loading exercise from URL: {requestUrl}");
                
                var response = await httpClient.GetAsync(requestUrl);
                _logger.LogInformation($"Exercise load response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Exercise JSON received: {json.Substring(0, Math.Min(200, json.Length))}...");
                    
                    Exercise = JsonSerializer.Deserialize<StudentExerciseDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    _logger.LogInformation($"Exercise deserialized: {Exercise?.Title}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to load exercise: {response.StatusCode}, Content: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading exercise: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }


        private async Task CheckCanStartAsync(int exerciseId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ИСПРАВЛЕНО: Используем правильный exerciseId
                var requestUrl = $"{apiBaseUrl}/api/student/exercises/{exerciseId}/can-start";
                _logger.LogInformation($"Checking can start from URL: {requestUrl}");
                
                var response = await httpClient.GetAsync(requestUrl);
                _logger.LogInformation($"Can start response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Can start response: {json}");
                    
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    CanStart = result.GetProperty("canStart").GetBoolean();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Can start check failed: {response.StatusCode}, Content: {errorContent}");
                    CanStart = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking can start: {ex.Message}");
                CanStart = false;
            }
        }

        private async Task StartAttemptAsync(int exerciseId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // ИСПРАВЛЕНО: Используем правильный exerciseId
                var requestUrl = $"{apiBaseUrl}/api/student/exercises/{exerciseId}/start";
                _logger.LogInformation($"Starting attempt from URL: {requestUrl}");
                
                var response = await httpClient.PostAsync(requestUrl, null);
                _logger.LogInformation($"Start attempt response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Start attempt response: {json}");
                    
                    var result = JsonSerializer.Deserialize<JsonElement>(json);
                    AttemptId = result.GetProperty("attemptId").GetInt32();
                    
                    StartTime = DateTime.Now;
                    EndTime = StartTime.AddMinutes(Exercise?.TimeLimit ?? 30);
                    
                    // Инициализируем Input с правильным exerciseId
                    Input.ExerciseId = exerciseId;
                    Input.AttemptId = AttemptId;
                    Input.Answers = Exercise?.Questions.Select(q => new StudentAnswerInputModel
                    {
                        QuestionId = q.Id
                    }).ToList() ?? new List<StudentAnswerInputModel>();
                    
                    _logger.LogInformation($"Attempt started successfully with ID: {AttemptId}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to start attempt: {response.StatusCode}, Content: {errorContent}");
                    throw new Exception($"Не удалось начать попытку: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error starting attempt: {ex.Message}");
                throw;
            }
        }
    }
}
