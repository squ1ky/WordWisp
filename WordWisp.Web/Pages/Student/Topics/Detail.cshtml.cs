using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Exercises; // Добавьте этот using
using WordWisp.Web.Models.Enums;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Student.Topics
{
    public class StudentTopicDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StudentTopicDetailModel> _logger;

        public StudentTopicDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<StudentTopicDetailModel> logger)
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

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "1" && userRole != "Student")
                return RedirectToPage("/Account/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Получаем информацию о топике
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/public/{id}");

                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

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
                    var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/student/topics/{id}/exercises");
                    if (exercisesResponse.IsSuccessStatusCode)
                    {
                        var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                        Exercises = JsonSerializer.Deserialize<List<StudentExerciseDto>>(exercisesJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<StudentExerciseDto>();
                        
                        _logger.LogInformation($"Loaded {Exercises.Count} exercises for topic {id}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to load exercises: {exercisesResponse.StatusCode}");
                    }
                }
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
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
