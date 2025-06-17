using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.Exercises;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Exercises
{
    public class CreateExerciseModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<CreateExerciseModel> _logger;

        public CreateExerciseModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<CreateExerciseModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public CreateExerciseInputModel Input { get; set; } = new();

        public TopicDto? Topic { get; set; }
        public List<MaterialDto> Materials { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int topicId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher")
                return RedirectToPage("/Auth/Login");

            try
            {
                await LoadTopicAndMaterialsAsync(topicId, token);
                
                if (Topic == null)
                {
                    ErrorMessage = "Тема не найдена или у вас нет к ней доступа";
                    return Page();
                }

                // Инициализируем с одним вопросом
                Input.Questions.Add(new CreateQuestionInputModel
                {
                    Order = 0,
                    Points = 1,
                    Answers = new List<CreateAnswerInputModel>
                    {
                        new() { Order = 0 },
                        new() { Order = 1 }
                    }
                });

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading create exercise page: {ex.Message}");
                ErrorMessage = $"Ошибка загрузки страницы: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int topicId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid)
            {
                await LoadTopicAndMaterialsAsync(topicId, token);
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    title = Input.Title,
                    description = Input.Description,
                    exerciseType = (int)Input.ExerciseType,
                    topicId = topicId,
                    materialId = Input.MaterialId,
                    timeLimit = Input.TimeLimit,
                    maxAttempts = Input.MaxAttempts,
                    passingScore = Input.PassingScore,
                    isActive = Input.IsActive,
                    order = Input.Order,
                    questions = Input.Questions.Select(q => new
                    {
                        question = q.Question,
                        questionImagePath = q.QuestionImagePath,
                        questionAudioPath = q.QuestionAudioPath,
                        order = q.Order,
                        points = q.Points,
                        answers = q.Answers.Select(a => new
                        {
                            answerText = a.AnswerText,
                            answerImagePath = a.AnswerImagePath,
                            isCorrect = a.IsCorrect,
                            order = a.Order
                        }).ToList()
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ОБНОВЛЕННЫЙ URL для нового API
                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/teacher/topics/{topicId}/exercises", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Упражнение успешно создано";
                    return RedirectToPage("/Teacher/Topics/Index", new { id = topicId });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка создания упражнения";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка создания упражнения";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка создания упражнения";
                    }
                    
                    await LoadTopicAndMaterialsAsync(topicId, token);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating exercise: {ex.Message}");
                ErrorMessage = "Не удалось подключиться к серверу";
                await LoadTopicAndMaterialsAsync(topicId, token);
                return Page();
            }
        }

        private async Task LoadTopicAndMaterialsAsync(int topicId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Загружаем тему
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{topicId}");
                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                // Загружаем материалы (опционально)
                try
                {
                    var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/{topicId}/materials");
                    if (materialsResponse.IsSuccessStatusCode)
                    {
                        var materialsJson = await materialsResponse.Content.ReadAsStringAsync();
                        Materials = JsonSerializer.Deserialize<List<MaterialDto>>(materialsJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<MaterialDto>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not load materials: {ex.Message}");
                    Materials = new List<MaterialDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading topic and materials: {ex.Message}");
            }
        }
    }
}
