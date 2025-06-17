using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Materials
{
    public class TeacherMaterialDetailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<TeacherMaterialDetailModel> _logger;

        public TeacherMaterialDetailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<TeacherMaterialDetailModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public MaterialDto? Material { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public string? AuthToken { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
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

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Material = JsonSerializer.Deserialize<MaterialDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return RedirectToPage("/Teacher/Topics/Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки материала";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading material {id}: {ex.Message}");
            }

            return Page();
        }
    }
}
