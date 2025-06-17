using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Topics
{
    public class TopicsIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TopicsIndexModel> _logger;

        public TopicsIndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<TopicsIndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicsPagedResult PagedTopics { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string SearchQuery { get; set; } = "";
        public int CurrentPage { get; set; } = 1;
        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        public int? CurrentUserId { get; set; }

        public async Task<IActionResult> OnGetAsync(string search = "", int page = 1)
        {
            var token = Request.Cookies["AuthToken"];
            IsAuthenticated = !string.IsNullOrEmpty(token);
            
            if (IsAuthenticated)
            {
                UserRole = _tokenService.GetUserRoleFromToken(token);
                var userIdString = _tokenService.GetUserIdFromToken(token);
                if (int.TryParse(userIdString, out int userId))
                {
                    CurrentUserId = userId;
                }
            }

            SearchQuery = search;
            CurrentPage = page;

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

                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (page > 1)
                    queryParams.Add($"page={page}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/public{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    PagedTopics = JsonSerializer.Deserialize<TopicsPagedResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new TopicsPagedResult();

                    // ДОБАВЛЯЕМ ЗАГРУЗКУ СЧЕТЧИКОВ ДЛЯ КАЖДОГО ТОПИКА
                    foreach (var topic in PagedTopics.Topics)
                    {
                        try
                        {
                            // Загружаем количество материалов
                            var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/topic/{topic.Id}");
                            if (materialsResponse.IsSuccessStatusCode)
                            {
                                var materialsJson = await materialsResponse.Content.ReadAsStringAsync();
                                var materials = JsonSerializer.Deserialize<List<object>>(materialsJson) ?? new List<object>();
                                topic.MaterialsCount = materials.Count;
                            }

                            // Загружаем количество упражнений в зависимости от роли пользователя
                            if (IsAuthenticated)
                            {
                                string exercisesEndpoint;
                                if (UserRole == "Teacher")
                                {
                                    // Для преподавателей используем teacher endpoint (показывает все упражнения)
                                    exercisesEndpoint = $"{apiBaseUrl}/api/teacher/topics/{topic.Id}/exercises";
                                }
                                else
                                {
                                    // Для студентов используем student endpoint (показывает только активные)
                                    exercisesEndpoint = $"{apiBaseUrl}/api/student/topics/{topic.Id}/exercises";
                                }

                                var exercisesResponse = await httpClient.GetAsync(exercisesEndpoint);
                                if (exercisesResponse.IsSuccessStatusCode)
                                {
                                    var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                                    var exercises = JsonSerializer.Deserialize<List<object>>(exercisesJson) ?? new List<object>();
                                    topic.ExercisesCount = exercises.Count;
                                }
                            }
                            else
                            {
                                // Для неавторизованных пользователей показываем 0 упражнений
                                topic.ExercisesCount = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Could not load counts for topic {topic.Id}: {ex.Message}");
                            // Оставляем значения по умолчанию
                        }
                    }
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки топиков";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading topics: {ex.Message}");
            }

            return Page();
        }
    }
}
