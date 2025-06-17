using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Student.Topics
{
    public class StudentTopicsIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<StudentTopicsIndexModel> _logger;

        public StudentTopicsIndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<StudentTopicsIndexModel> logger)
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

        public async Task<IActionResult> OnGetAsync(string search = "", int page = 1)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Student") // Только для студентов
                return RedirectToPage("/Account/Index");

            SearchQuery = search;
            CurrentPage = page;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

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

                            // Загружаем количество упражнений для студентов (только активные)
                            var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/student/topics/{topic.Id}/exercises");
                            if (exercisesResponse.IsSuccessStatusCode)
                            {
                                var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                                var exercises = JsonSerializer.Deserialize<List<object>>(exercisesJson) ?? new List<object>();
                                topic.ExercisesCount = exercises.Count;
                            }
                            else
                            {
                                // Если не удалось загрузить упражнения, ставим 0
                                topic.ExercisesCount = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Could not load counts for topic {topic.Id}: {ex.Message}");
                            // Оставляем значения по умолчанию
                            topic.MaterialsCount = 0;
                            topic.ExercisesCount = 0;
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
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
