using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Account
{
    public class AccountIndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AccountIndexModel> _logger;

        public AccountIndexModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<AccountIndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        public JsonElement? UserStats { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null)
                return RedirectToPage("/Auth/Login");

            if (TempData.ContainsKey("SuccessMessage"))
                SuccessMessage = TempData["SuccessMessage"]?.ToString();

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{userId}/stats");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    UserStats = JsonSerializer.Deserialize<JsonElement>(json);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки данных профиля";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading user stats: {ex.Message}");
            }

            return Page();
        }
    }
}
