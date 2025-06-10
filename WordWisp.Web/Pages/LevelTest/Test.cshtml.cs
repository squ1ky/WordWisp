using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.LevelTest;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.LevelTest
{
    public class TestModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public TestModel(IHttpClientFactory httpClientFactory,
                        IConfiguration configuration,
                        ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [BindProperty(SupportsGet = true)]
        public int TestId { get; set; }

        [BindProperty]
        public LevelTestQuestionModel? CurrentQuestion { get; set; }

        public LevelTestSessionModel? TestSession { get; set; }
        public string? ErrorMessage { get; set; }
        public string CurrentSection { get; set; } = "Grammar";

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

                var testResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/level-test/active");
                if (testResponse.IsSuccessStatusCode)
                {
                    var testContent = await testResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(testContent) && testContent != "null")
                    {
                        TestSession = JsonSerializer.Deserialize<LevelTestSessionModel>(testContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (TestSession == null || TestSession.Id != TestId)
                        {
                            TempData["Error"] = "Тест не найден или недоступен.";
                            return RedirectToPage("Index");
                        }

                        CurrentSection = TestSession.CurrentSection;
                    }
                    else
                    {
                        TempData["Error"] = "Активный тест не найден.";
                        return RedirectToPage("Index");
                    }
                }
                else if (testResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    TempData["Error"] = "Тест не найден.";
                    return RedirectToPage("Index");
                }

                await LoadNextQuestionAsync(httpClient, apiBaseUrl);
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAnswerAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var questionIdStr = Request.Form["QuestionId"];
            var selectedAnswer = Request.Form["SelectedAnswer"];

            if (!int.TryParse(questionIdStr, out int questionId) || questionId == 0)
            {
                TempData["Error"] = "Ошибка: ID вопроса не найден";
                return RedirectToPage(new { testId = TestId });
            }

            if (string.IsNullOrEmpty(selectedAnswer))
            {
                TempData["Error"] = "Пожалуйста, выберите ответ";
                return RedirectToPage(new { testId = TestId });
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    QuestionId = questionId,
                    SelectedAnswer = selectedAnswer.ToString()
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var submitResponse = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/{TestId}/answers", content);

                if (submitResponse.IsSuccessStatusCode)
                {
                    return RedirectToPage(new { testId = TestId });
                }
                else if (submitResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await submitResponse.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Ошибка при отправке ответа: {submitResponse.StatusCode}";
                    return RedirectToPage(new { testId = TestId });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Не удалось подключиться к серверу";
                return RedirectToPage(new { testId = TestId });
            }
        }

        public async Task<IActionResult> OnPostCompleteTestAsync()
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

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/{TestId}/complete", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("Results", new { testId = TestId });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка при завершении теста";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        private async Task LoadNextQuestionAsync(HttpClient httpClient, string apiBaseUrl)
        {
            var sections = new[] { "Grammar", "Vocabulary", "Reading" };

            foreach (var section in sections)
            {
                var questionResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/level-test/{TestId}/questions/next?section={section}");

                if (questionResponse.IsSuccessStatusCode)
                {
                    var questionContent = await questionResponse.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(questionContent) || questionContent == "null")
                    {
                        continue;
                    }

                    try
                    {
                        CurrentQuestion = JsonSerializer.Deserialize<LevelTestQuestionModel>(questionContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (CurrentQuestion != null)
                        {
                            CurrentSection = section;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Deserialization error: {ex.Message}");
                    }
                }
            }

            CurrentQuestion = null;
        }

        private async Task<IActionResult> HandleSectionCompleted(HttpClient httpClient, string apiBaseUrl)
        {
            await LoadNextQuestionAsync(httpClient, apiBaseUrl);

            if (CurrentQuestion == null)
            {
                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/level-test/{TestId}/complete", null);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToPage("Results", new { testId = TestId });
                }
                else
                {
                    ErrorMessage = "Ошибка при завершении теста";
                    return Page();
                }
            }

            return Page();
        }
    }
}
