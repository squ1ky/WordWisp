using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Materials;
using WordWisp.Web.Models.Materials;
using WordWisp.Web.Models.Enums;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Materials
{
    public class EditMaterialModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EditMaterialModel> _logger;

        public EditMaterialModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<EditMaterialModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public EditMaterialInputModel Input { get; set; } = new();

        public MaterialDto? Material { get; set; }
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

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/materials/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Material = JsonSerializer.Deserialize<MaterialDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (Material != null)
                    {
                        Input.Title = Material.Title;
                        Input.Description = Material.Description;
                        Input.Content = Material.Content;
                        Input.ExternalUrl = Material.ExternalUrl;
                        Input.IsPublic = Material.IsPublic;
                        Input.Order = Material.Order;
                        Input.CurrentFilePath = Material.FilePath;
                        Input.CurrentFileName = Material.OriginalFileName;
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

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                await LoadMaterialData(id);
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

                // Определяем, нужно ли обновлять файл
                bool hasNewFile = Input.File != null && Input.File.Length > 0;

                if (hasNewFile && (Material?.MaterialType == MaterialType.Image || Material?.MaterialType == MaterialType.Audio))
                {
                    // Обновляем материал с новым файлом
                    using var formData = new MultipartFormDataContent();
                    formData.Add(new StringContent(Input.Title), "Title");
                    formData.Add(new StringContent(Input.Description ?? ""), "Description");
                    formData.Add(new StringContent(Input.Content ?? ""), "Content");
                    formData.Add(new StringContent(Input.ExternalUrl ?? ""), "ExternalUrl");
                    formData.Add(new StringContent(Input.IsPublic.ToString()), "IsPublic");
                    formData.Add(new StringContent(Input.Order.ToString()), "Order");

                    var fileContent = new StreamContent(Input.File.OpenReadStream());
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Input.File.ContentType);
                    formData.Add(fileContent, "File", Input.File.FileName);

                    var response = await httpClient.PutAsync($"{apiBaseUrl}/api/materials/{id}/upload", formData);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Материал успешно обновлен";
                        return RedirectToPage("/Teacher/Materials/Detail", new { id });
                    }
                    else
                    {
                        await HandleErrorResponse(response);
                    }
                }
                else
                {
                    // Обновляем материал без файла
                    var requestData = new
                    {
                        title = Input.Title?.Trim(),
                        description = Input.Description?.Trim(),
                        content = Input.Content?.Trim(),
                        externalUrl = Input.ExternalUrl?.Trim(),
                        isPublic = Input.IsPublic,
                        order = Input.Order
                    };

                    var json = JsonSerializer.Serialize(requestData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PutAsync($"{apiBaseUrl}/api/materials/{id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Материал успешно обновлен";
                        return RedirectToPage("/Teacher/Materials/Detail", new { id });
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
                _logger.LogError($"Error updating material {id}: {ex.Message}");
            }

            await LoadMaterialData(id);
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
                        ErrorMessage = messageElement.GetString() ?? "Ошибка обновления материала";
                    }
                    else
                    {
                        ErrorMessage = "Ошибка обновления материала";
                    }
                }
                catch
                {
                    ErrorMessage = "Ошибка обновления материала";
                }
            }
        }

        private async Task LoadMaterialData(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return;

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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading material data: {ex.Message}");
            }
        }
    }
}
