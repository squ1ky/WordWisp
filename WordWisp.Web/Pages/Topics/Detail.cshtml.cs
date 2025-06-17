using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Exercises; // Добавьте этот using
using WordWisp.Web.Models.Enums;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Topics
{
    public class TopicDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TopicDetailModel> _logger;

        public TopicDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<TopicDetailModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicDto? Topic { get; set; }
        public List<MaterialDto> Materials { get; set; } = new();
        public List<StudentExerciseDto> Exercises { get; set; } = new(); // Используйте StudentExerciseDto
        public string? ErrorMessage { get; set; }
        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        public bool IsOwner { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            IsAuthenticated = !string.IsNullOrEmpty(token);
            
            if (IsAuthenticated)
            {
                UserRole = _tokenService.GetUserRoleFromToken(token);
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                // Добавляем токен если пользователь авторизован
                if (IsAuthenticated)
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                // Получаем информацию о топике
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/public/{id}");

                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // Проверяем, является ли пользователь владельцем топика
                    if (IsAuthenticated && Topic != null)
                    {
                        var currentUserId = _tokenService.GetUserIdFromToken(token);
                        IsOwner = currentUserId != null && int.Parse(currentUserId) == Topic.CreatedBy;
                    }

                    // Загружаем материалы
                    var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/topic/{id}");
                    if (materialsResponse.IsSuccessStatusCode)
                    {
                        var materialsJson = await materialsResponse.Content.ReadAsStringAsync();
                        Materials = JsonSerializer.Deserialize<List<MaterialDto>>(materialsJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<MaterialDto>();
                    }

                    // ДОБАВЛЯЕМ ЗАГРУЗКУ УПРАЖНЕНИЙ
                    if (IsAuthenticated && UserRole == "Student")
                    {
                        // Для студентов используем студенческий API
                        var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/student/topics/{id}/exercises");
                        if (exercisesResponse.IsSuccessStatusCode)
                        {
                            var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                            Exercises = JsonSerializer.Deserialize<List<StudentExerciseDto>>(exercisesJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<StudentExerciseDto>();
                            
                            _logger.LogInformation($"Loaded {Exercises.Count} exercises for student");
                        }
                    }
                    else if (IsAuthenticated && UserRole == "Teacher")
                    {
                        // Для преподавателей используем преподавательский API
                        var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/teacher/topics/{id}/exercises");
                        if (exercisesResponse.IsSuccessStatusCode)
                        {
                            var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                            var teacherExercises = JsonSerializer.Deserialize<List<ExerciseDto>>(exercisesJson, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            }) ?? new List<ExerciseDto>();

                            // Конвертируем в StudentExerciseDto для единообразного отображения
                            Exercises = teacherExercises.Where(e => e.IsActive).Select(e => new StudentExerciseDto
                            {
                                Id = e.Id,
                                Title = e.Title,
                                Description = e.Description,
                                ExerciseType = e.ExerciseType,
                                TopicId = e.TopicId,
                                TopicTitle = e.TopicTitle,
                                TimeLimit = e.TimeLimit,
                                MaxAttempts = e.MaxAttempts,
                                PassingScore = e.PassingScore,
                                Questions = new List<StudentExerciseQuestionDto>() // Пустой список для публичного просмотра
                            }).ToList();
                            
                            _logger.LogInformation($"Loaded {Exercises.Count} exercises for teacher");
                        }
                    }
                    else
                    {
                        // Для неавторизованных пользователей показываем заглушку
                        Exercises = new List<StudentExerciseDto>();
                    }

                    // Обновляем счетчики в Topic
                    if (Topic != null)
                    {
                        Topic.MaterialsCount = Materials.Count;
                        Topic.ExercisesCount = Exercises.Count;
                    }
                }
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized && IsAuthenticated)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки топика";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading topic {id}: {ex.Message}");
            }

            return Page();
        }
    }
}
