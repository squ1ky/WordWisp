using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Topics
{
    public class EditTopicModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EditTopicModel> _logger;

        public EditTopicModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<EditTopicModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public EditTopicInputModel Input { get; set; } = new();

        public TopicDto? Topic { get; set; }
        public string? ErrorMessage { get; set; }
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

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Topic != null)
                    {
                        Input.Title = Topic.Title;
                        Input.Description = Topic.Description;
                        Input.IsPublic = Topic.IsPublic;
                    }
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
                    ErrorMessage = "Ошибка загрузки топика";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading topic {id}: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicData(id);
                return Page();
            }

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            AuthToken = token;

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher")
                return RedirectToPage("/Account/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new UpdateTopicRequest
                {
                    Title = Input.Title?.Trim() ?? "",
                    Description = Input.Description?.Trim(),
                    IsPublic = Input.IsPublic
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{apiBaseUrl}/api/topics/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Топик успешно обновлен";
                    return RedirectToPage("/Teacher/Topics/Detail", new { id });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    ErrorMessage = "У вас нет прав для редактирования этого топика";
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка обновления топика";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка обновления топика";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка обновления топика";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error updating topic {id}: {ex.Message}");
            }

            await LoadTopicData(id);
            return Page();
        }

        private async Task LoadTopicData(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Topic = JsonSerializer.Deserialize<TopicDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading topic data: {ex.Message}");
            }
        }
    }

    public class EditTopicInputModel
    {
        [Required(ErrorMessage = "Название топика обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}
