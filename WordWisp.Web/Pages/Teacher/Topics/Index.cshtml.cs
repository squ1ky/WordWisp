using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Topics
{
    public class TeacherTopicsIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TeacherTopicsIndexModel> _logger;

        public TeacherTopicsIndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<TeacherTopicsIndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicsPagedResult PagedTopics { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string SearchQuery { get; set; } = "";
        public int CurrentPage { get; set; } = 1;

        public string? AuthToken { get; set; }
        

        public async Task<IActionResult> OnGetAsync(string search = "", int page = 1)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            AuthToken = token;
            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher") // Только для преподавателей
                return RedirectToPage("/Account/Index");

            if (TempData.ContainsKey("SuccessMessage"))
                SuccessMessage = TempData["SuccessMessage"]?.ToString();

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
                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    PagedTopics = JsonSerializer.Deserialize<TopicsPagedResult>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new TopicsPagedResult();

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

                            // Загружаем количество упражнений
                            var exercisesResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/teacher/topics/{topic.Id}/exercises");
                            if (exercisesResponse.IsSuccessStatusCode)
                            {
                                var exercisesJson = await exercisesResponse.Content.ReadAsStringAsync();
                                var exercises = JsonSerializer.Deserialize<List<object>>(exercisesJson) ?? new List<object>();
                                topic.ExercisesCount = exercises.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Could not load counts for topic {topic.Id}: {ex.Message}");
                            // Оставляем значения по умолчанию
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
