using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Dictionaries;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Dictionaries
{
    public class DictionaryDetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public DictionaryDetailsModel(IHttpClientFactory httpClientFactory,
                                      IConfiguration configuration,
                                      ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        public DictionaryDetailDto? Dictionary { get; set; }
        public bool IsOwner { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (TempData.ContainsKey("SuccessMessage"))
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            var token = Request.Cookies["AuthToken"];
            var userId = _tokenService.GetUserIdFromToken(token);

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                if (!string.IsNullOrEmpty(token))
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{userId ?? "0"}/dictionaries/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Dictionary = JsonSerializer.Deserialize<DictionaryDetailDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Dictionary != null)
                    {
                        IsOwner = !string.IsNullOrEmpty(userId);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "Словарь не найден";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    ErrorMessage = "У вас нет доступа к этому словарю";
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки словаря";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteWordAsync(int id, int wordId)
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

                var response = await httpClient.DeleteAsync($"{apiBaseUrl}/api/users/{userId}/dictionaries/{id}/words/{wordId}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Слово удалено";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка удаления слова";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Не удалось подключиться к серверу";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteDictionaryAsync(int id)
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

                var response = await httpClient.DeleteAsync($"{apiBaseUrl}/api/users/{userId}/dictionaries/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Словарь удален";
                    return RedirectToPage("/Dictionaries/Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка удаления словаря";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Не удалось подключиться к серверу";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostToggleVisibilityAsync(int id)
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

                var response = await httpClient.PatchAsync(
                    $"{apiBaseUrl}/api/users/{userId}/dictionaries/{id}/toggle-visibility",
                    null);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Видимость словаря изменена";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    TempData["ErrorMessage"] = "Ошибка изменения видимости словаря";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Toggle visibility error: {ex.Message}");
                TempData["ErrorMessage"] = "Ошибка подключения к серверу";
            }

            return RedirectToPage(new { id });
        }
    }
}
