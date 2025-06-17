using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Topics;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Teacher.Topics
{
    public class CreateTopicModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<CreateTopicModel> _logger;

        public CreateTopicModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<CreateTopicModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public CreateTopicInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher") // Только для преподавателей
                return RedirectToPage("/Account/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var userRole = _tokenService.GetUserRoleFromToken(token);
            if (userRole != "Teacher")
                return RedirectToPage("/Account/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new CreateTopicRequest
                {
                    Title = Input.Title?.Trim() ?? "",
                    Description = Input.Description?.Trim(),
                    IsPublic = Input.IsPublic
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/topics", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Топик успешно создан";
                    return RedirectToPage("/Teacher/Topics/Index");
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка создания топика";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка создания топика";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка создания топика";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error creating topic: {ex.Message}");
            }

            return Page();
        }
    }

    public class CreateTopicInputModel
    {
        [Required(ErrorMessage = "Название топика обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = true;
    }
}
