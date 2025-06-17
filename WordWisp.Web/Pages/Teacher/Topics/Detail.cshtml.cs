using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Exercises;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Topics
{
    public class TeacherTopicDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TeacherTopicDetailModel> _logger;

        public TeacherTopicDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<TeacherTopicDetailModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicDto? Topic { get; set; }
        public List<MaterialDto> Materials { get; set; } = new();
        public List<ExerciseDto> Exercises { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "2" && userRole != "Teacher")
                return RedirectToPage("/Account/Index");

            if (TempData.ContainsKey("SuccessMessage"))
                SuccessMessage = TempData["SuccessMessage"]?.ToString();

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Получаем информацию о топике
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{id}");

                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/topic/{id}");
                    if (materialsResponse.IsSuccessStatusCode)
                    {
                        var materialsJson = await materialsResponse.Content.ReadAsStringAsync();
                        Materials = JsonSerializer.Deserialize<List<MaterialDto>>(materialsJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<MaterialDto>();
                    }

                    var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/teacher/topics/{id}/exercises");
                    if (exercisesResponse.IsSuccessStatusCode)
                    {
                        var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                        Exercises = JsonSerializer.Deserialize<List<ExerciseDto>>(exercisesJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<ExerciseDto>();

                        _logger.LogInformation($"Loaded {Exercises.Count} exercises for topic {id}");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to load exercises: {exercisesResponse.StatusCode}");
                    }
                    
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
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToPage("/Teacher/Topics/Index");
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
