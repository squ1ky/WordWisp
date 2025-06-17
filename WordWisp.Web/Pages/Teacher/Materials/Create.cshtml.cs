using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Models.Materials;
using WordWisp.Web.Models.Enums;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Materials
{
    public class CreateMaterialModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<CreateMaterialModel> _logger;

        public CreateMaterialModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<CreateMaterialModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public CreateMaterialInputModel Input { get; set; } = new();

        public TopicDto? Topic { get; set; }
        public string? ErrorMessage { get; set; }
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

            Input.TopicId = topicId;

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
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading topic {topicId}: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTopicData();
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

                // Определяем тип запроса в зависимости от типа материала
                if (Input.MaterialType == MaterialType.Image || Input.MaterialType == MaterialType.Audio)
                {
                    // Для файлов используем multipart/form-data
                    if (Input.File == null || Input.File.Length == 0)
                    {
                        ModelState.AddModelError("Input.File", "Файл обязателен для данного типа материала");
                        await LoadTopicData();
                        return Page();
                    }

                    using var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(Input.Title), "Title");
                    formData.Add(new StringContent(Input.Description ?? ""), "Description");
                    formData.Add(new StringContent(((int)Input.MaterialType).ToString()), "MaterialType");
                    formData.Add(new StringContent(Input.IsPublic.ToString()), "IsPublic");
                    formData.Add(new StringContent(Input.Order.ToString()), "Order");
                    formData.Add(new StringContent(Input.TopicId.ToString()), "TopicId");

                    var fileContent = new StreamContent(Input.File.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Input.File.ContentType);
                    formData.Add(fileContent, "File", Input.File.FileName);

                    var response = await httpClient.PostAsync($"{apiBaseUrl}/api/materials/upload", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Материал успешно создан";
                        return Redirect($"/teacher/topics/{Input.TopicId}/materials");
                    }
                    else
                    {
                        await HandleErrorResponse(response);
                    }
                }
                else
                {
                    // Для текста и видео используем JSON
                    var requestData = new
                    {
                        title = Input.Title?.Trim(),
                        description = Input.Description?.Trim(),
                        materialType = (int)Input.MaterialType,
                        content = Input.Content?.Trim(),
                        externalUrl = Input.ExternalUrl?.Trim(),
                        isPublic = Input.IsPublic,
                        order = Input.Order,
                        topicId = Input.TopicId
                    };

                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync($"{apiBaseUrl}/api/materials", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Материал успешно создан";
                        return Redirect($"/teacher/topics/{Input.TopicId}/materials");
                    }
                    else
                    {
                        await HandleErrorResponse(response);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error creating material: {ex.Message}");
            }

            await LoadTopicData();
            return Page();
        }

        private async Task HandleErrorResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Сессия истекла";
            }
            else
            {
                try
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                    if (errorData.TryGetProperty("message", out var messageElement))
                    {
                        ErrorMessage = messageElement.GetString() ?? "Ошибка создания материала";
                    }
                    else
                    {
                        ErrorMessage = "Ошибка создания материала";
                    }
                }
                catch
                {
                    ErrorMessage = "Ошибка создания материала";
                }
            }
        }

        private async Task LoadTopicData()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/topics/teacher/{Input.TopicId}");

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
}
