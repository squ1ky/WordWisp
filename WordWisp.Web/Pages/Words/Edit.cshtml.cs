using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Words;
using WordWisp.Web.Models.Words;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Words
{
    public class EditWordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;

        public EditWordModel(IHttpClientFactory httpClientFactory,
                             IConfiguration configuration,
                             ITokenService tokenService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [BindProperty]
        public EditWordInputModel Input { get; set; } = new();

        public int DictionaryId { get; set; }
        public int WordId { get; set; }
        public string? DictionaryName { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int dictionaryId, int wordId)
        {
            DictionaryId = dictionaryId;
            WordId = wordId;

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

                var wordResponse = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{userId}/dictionaries/{dictionaryId}/words/{wordId}");

                if (wordResponse.IsSuccessStatusCode)
                {
                    var json = await wordResponse.Content.ReadAsStringAsync();
                    var word = JsonSerializer.Deserialize<WordDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (word != null)
                    {
                        Input.Term = word.Term;
                        Input.Definition = word.Definition;
                        Input.Transcription = word.Transcription;
                        Input.Example = word.Example;
                    }

                    await LoadDictionaryName(dictionaryId, token, userId);
                }
                else if (wordResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else if (wordResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return RedirectToPage("/Dictionaries/Details", new { id = dictionaryId });
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки слова";
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int dictionaryId, int wordId)
        {
            DictionaryId = dictionaryId;
            WordId = wordId;

            if (!ModelState.IsValid)
            {
                var token = Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(token))
                {
                    var userId = _tokenService.GetUserIdFromToken(token);
                    if (userId != null)
                    {
                        await LoadDictionaryName(dictionaryId, token, userId);
                    }
                }
                return Page();
            }

            var authToken = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(authToken))
            {
                return RedirectToPage("/Auth/Login");
            }

            var currentUserId = _tokenService.GetUserIdFromToken(authToken);
            if (currentUserId == null)
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

                var response = await httpClient.PutAsync($"{apiBaseUrl}/api/users/{currentUserId}/dictionaries/{dictionaryId}/words/{wordId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Слово обновлено успешно";
                    return RedirectToPage("/Dictionaries/Details", new { id = dictionaryId });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка обновления слова";
                    await LoadDictionaryName(dictionaryId, authToken, currentUserId);
                }
            }
            catch (Exception)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                await LoadDictionaryName(dictionaryId, authToken, currentUserId);
            }

            return Page();
        }

        private async Task LoadDictionaryName(int dictionaryId, string token, string userId)
        {
            try
            {
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
