using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Exercises;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.Exercises;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Exercises
{
    public class EditExerciseModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EditExerciseModel> _logger;

        public EditExerciseModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ITokenService tokenService,
            ILogger<EditExerciseModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public EditExerciseInputModel Input { get; set; } = new();

        public ExerciseDto? Exercise { get; set; }
        public TopicDto? Topic { get; set; }
        public List<MaterialDto> Materials { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher")
                return RedirectToPage("/Auth/Login");

            if (TempData.ContainsKey("SuccessMessage"))
                SuccessMessage = TempData["SuccessMessage"]?.ToString();

            try
            {
                await LoadExerciseDataAsync(id, token);
                
                if (Exercise == null)
                {
                    TempData["ErrorMessage"] = "Упражнение не найдено";
                    return RedirectToPage("/Teacher/Exercises");
                }

                PopulateInputFromExercise();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading edit exercise page: {ex.Message}");
                ErrorMessage = $"Ошибка загрузки страницы: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid)
            {
                await LoadExerciseDataAsync(id, token);
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
                    materialId = Input.MaterialId,
                    timeLimit = Input.TimeLimit,
                    maxAttempts = Input.MaxAttempts,
                    passingScore = Input.PassingScore,
                    isActive = Input.IsActive,
                    order = Input.Order,
                    questions = Input.Questions.Select(q => new
                    {
                        id = q.Id,
                        question = q.Question,
                        questionImagePath = q.QuestionImagePath,
                        questionAudioPath = q.QuestionAudioPath,
                        order = q.Order,
                        points = q.Points,
                        answers = q.Answers.Select(a => new
                        {
                            id = a.Id,
                            answerText = a.AnswerText,
                            answerImagePath = a.AnswerImagePath,
                            isCorrect = a.IsCorrect,
                            order = a.Order
                        }).ToList()
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{apiBaseUrl}/api/teacher/exercises/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Упражнение успешно обновлено";
                    return RedirectToPage("/Teacher/Exercises/Edit", new { id });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка обновления упражнения";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка обновления упражнения";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка обновления упражнения";
                    }
                    
                    await LoadExerciseDataAsync(id, token);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating exercise: {ex.Message}");
                ErrorMessage = "Не удалось подключиться к серверу";
                await LoadExerciseDataAsync(id, token);
                return Page();
            }
        }

        private async Task LoadExerciseDataAsync(int exerciseId, string token)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Загружаем упражнение
                var exerciseResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/teacher/exercises/{exerciseId}");
                if (exerciseResponse.IsSuccessStatusCode)
                {
                    var exerciseJson = await exerciseResponse.Content.ReadAsStringAsync();
                    Exercise = JsonSerializer.Deserialize<ExerciseDto>(exerciseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Exercise != null)
                    {
                        // Загружаем тему
                        var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/{Exercise.TopicId}");
                        if (topicResponse.IsSuccessStatusCode)
                        {
                            var topicJson = await topicResponse.Content.ReadAsStringAsync();
                            Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }

                        // Загружаем материалы темы
                        var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/topic/{Exercise.TopicId}");
                        if (materialsResponse.IsSuccessStatusCode)
                        {
                            var materialsJson = await materialsResponse.Content.ReadAsStringAsync();
                            Materials = JsonSerializer.Deserialize<List<MaterialDto>>(materialsJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<MaterialDto>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading exercise data: {ex.Message}");
            }
        }

        private void PopulateInputFromExercise()
        {
            if (Exercise == null) return;

            Input.Id = Exercise.Id;
            Input.Title = Exercise.Title;
            Input.Description = Exercise.Description;
            Input.ExerciseType = Exercise.ExerciseType;
            Input.MaterialId = Exercise.MaterialId;
            Input.TimeLimit = Exercise.TimeLimit;
            Input.MaxAttempts = Exercise.MaxAttempts;
            Input.PassingScore = Exercise.PassingScore;
            Input.IsActive = Exercise.IsActive;
            Input.Order = Exercise.Order;

            Input.Questions = Exercise.Questions.Select(q => new EditQuestionInputModel
            {
                Id = q.Id,
                Question = q.Question,
                QuestionImagePath = q.QuestionImagePath,
                QuestionAudioPath = q.QuestionAudioPath,
                Points = q.Points,
                Order = q.Order,
                Answers = q.Answers.Select(a => new EditAnswerInputModel
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText,
                    AnswerImagePath = a.AnswerImagePath,
                    IsCorrect = a.IsCorrect,
                    Order = a.Order
                }).ToList()
            }).ToList();
        }
    }
}
