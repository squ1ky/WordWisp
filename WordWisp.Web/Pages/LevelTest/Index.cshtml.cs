using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.LevelTest;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.LevelTest
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public IndexModel(IHttpClientFactory httpClientFactory,
                         IConfiguration configuration,
                         ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public bool CanStartTest { get; set; }
        public LevelTestSessionModel? ActiveTest { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var eligibilityResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/level-test/eligibility");
                if (eligibilityResponse.IsSuccessStatusCode)
                {
                    var eligibilityContent = await eligibilityResponse.Content.ReadAsStringAsync();
                    var eligibilityResult = JsonSerializer.Deserialize<EligibilityResponse>(eligibilityContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    CanStartTest = eligibilityResult?.Eligible ?? false;
                }
                else if (eligibilityResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }

                var activeTestResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/level-test/active");
                if (activeTestResponse.IsSuccessStatusCode)
                {
                    var activeTestContent = await activeTestResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(activeTestContent) && activeTestContent != "null")
                    {
                        ActiveTest = JsonSerializer.Deserialize<LevelTestSessionModel>(activeTestContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                }
                else if (activeTestResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostStartAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/start", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var session = JsonSerializer.Deserialize<LevelTestSessionModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (session != null)
                    {
                        return RedirectToPage("Test", new { testId = session.Id });
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Невозможно начать тест. Проверьте доступность.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Не удалось подключиться к серверу";
            }

            return RedirectToPage();
        }
    }
}
