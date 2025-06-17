using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Materials
{
    public class MaterialsIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<MaterialsIndexModel> _logger;

        public MaterialsIndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<MaterialsIndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public TopicDto? Topic { get; set; }
        public List<MaterialDto> Materials { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? AuthToken { get; set; }

        public async Task<IActionResult> OnGetAsync(int topicId)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            AuthToken = token;

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher")
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
                var topicResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{topicId}");
                if (topicResponse.IsSuccessStatusCode)
                {
                    var topicJson = await topicResponse.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(topicJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (topicResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToPage("/Teacher/Topics/Index");
                }

                // Получаем материалы топика
                var materialsResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/topic/{topicId}");
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
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading materials for topic {topicId}: {ex.Message}");
            }

            return Page();
        }
    }
}
