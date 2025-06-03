using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.Words;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Words
{
    public class CreateWordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public CreateWordModel(IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [BindProperty]
        public CreateWordInputModel Input { get; set; } = new();

        public int DictionaryId { get; set; }
        public string? DictionaryName { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int dictionaryId)
        {
            DictionaryId = dictionaryId;

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (TempData.ContainsKey("SuccessMessage"))
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
            }

            await LoadDictionaryName(dictionaryId, token);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int dictionaryId, bool addAnother = false)
        {
            DictionaryId = dictionaryId;

            if (!ModelState.IsValid)
            {
                var token = Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    await LoadDictionaryName(dictionaryId, token);
                }
                return Page();
            }

            var authToken = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToPage("/Auth/Login");
            }

            var userId = _tokenService.GetUserIdFromToken(authToken);
            if (userId == null)
            {
                return RedirectToPage("/Auth/Login");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                var requestData = new
                {
                    term = Input.Term,
                    definition = Input.Definition,
                    transcription = Input.Transcription,
                    example = Input.Example
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/users/{userId}/dictionaries/{dictionaryId}/words", content);

                if (response.IsSuccessStatusCode)
                {
                    if (addAnother)
                    {
                        TempData["SuccessMessage"] = "Слово добавлено! Добавьте еще одно.";
                        Input = new CreateWordInputModel();
                        return RedirectToPage(new { dictionaryId });
                    }
                    else
                    {
                        TempData["SuccessMessage"] = "Слово добавлено успешно";
                        return RedirectToPage("/Dictionaries/Details", new { id = dictionaryId });
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка добавления слова";
                    await LoadDictionaryName(dictionaryId, authToken);
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                await LoadDictionaryName(dictionaryId, authToken);
            }

            return Page();
        }

        private async Task LoadDictionaryName(int dictionaryId, string token)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken(token);
                if (userId == null) return;

                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{userId}/dictionaries/{dictionaryId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dictionary = JsonSerializer.Deserialize<JsonElement>(json);
                    DictionaryName = dictionary.GetProperty("name").GetString();
                }
            }
            catch
            {

            }
        }
    }
}
