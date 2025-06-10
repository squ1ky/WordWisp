using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.LevelTest;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.LevelTest
{
    public class ResultsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public ResultsModel(IHttpClientFactory httpClientFactory,
                           IConfiguration configuration,
                           ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [BindProperty(SupportsGet = true)]
        public int TestId { get; set; }

        public LevelTestResultModel? TestResult { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userId = _tokenService.GetUserIdFromToken(token);
            if (userId == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/{TestId}/complete", null);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    TestResult = JsonSerializer.Deserialize<LevelTestResultModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["Error"] = "Тест не найден или уже завершен.";
                    return RedirectToPage("Index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var historyResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/level-test/history");

                    if (historyResponse.IsSuccessStatusCode)
                    {
                        var historyContent = await historyResponse.Content.ReadAsStringAsync();
                        var history = JsonSerializer.Deserialize<List<LevelTestResultModel>>(historyContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        TestResult = history?.FirstOrDefault(t => t.TestId == TestId);

                        if (TestResult == null)
                        {
                            ErrorMessage = "Результаты теста не найдены.";
                        }
                    }
                    else
                    {
                        ErrorMessage = "Не удалось получить результаты теста.";
                    }
                }
                else
                {
                    ErrorMessage = "Ошибка при получении результатов теста.";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            if (TestResult == null && string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Результаты теста не найдены.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSendCertificateAsync()
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

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/{TestId}/send-certificate", null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Сертификат отправлен на вашу почту!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    TempData["Error"] = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Ошибка при отправке сертификата";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Не удалось подключиться к серверу";
                Console.WriteLine($"Exception: {ex}");
            }

            return RedirectToPage(new { testId = TestId });
        }
    }
}
