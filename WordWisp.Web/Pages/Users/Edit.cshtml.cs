using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.DTOs.Users;
using WordWisp.Web.Models.Users;
using WordWisp.Web.Services.Interfaces;

namespace WordWisp.Web.Pages.Users
{
    public class EditUserModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly ILogger<EditUserModel> _logger;

        public EditUserModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, ITokenService tokenService, ILogger<EditUserModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _logger = logger;
        }

        [BindProperty]
        public EditUserInputModel Input { get; set; } = new();

        [BindProperty]
        public ChangePasswordInputModel PasswordInput { get; set; } = new();

        public UserDto? User { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var currentUserId = _tokenService.GetUserIdFromToken(token);
            if (currentUserId == null || int.Parse(currentUserId) != id)
                return RedirectToPage("/Auth/Login");

            if (TempData.ContainsKey("SuccessMessage"))
                SuccessMessage = TempData["SuccessMessage"]?.ToString();

            if (TempData.ContainsKey("ErrorMessage"))
                ErrorMessage = TempData["ErrorMessage"]?.ToString();

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    User = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (User != null)
                    {
                        Input.Username = User.Username;
                        Input.Name = User.Name;
                        Input.Surname = User.Surname;
                        Input.Email = User.Email;
                        Input.Role = User.Role;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = "Ошибка загрузки данных пользователя";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error loading user data: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateAsync(int id)
        {
            _logger.LogInformation($"OnPostUpdateAsync called for user {id}");

            // Создаем новый ModelState только для основных полей
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(Input);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(Input, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ModelState.AddModelError($"Input.{memberName}", validationResult.ErrorMessage ?? "Ошибка валидации");
                    }
                }
                await LoadUserData(id);
                return Page();
            }

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var currentUserId = _tokenService.GetUserIdFromToken(token);
            if (currentUserId == null || int.Parse(currentUserId) != id)
                return RedirectToPage("/Auth/Login");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    username = Input.Username?.Trim(),
                    name = Input.Name?.Trim(),
                    surname = Input.Surname?.Trim(),
                    email = Input.Email?.Trim(),
                    role = (int)Input.Role
                };

                var json = JsonSerializer.Serialize(requestData);
                _logger.LogInformation($"Sending PUT request to {apiBaseUrl}/api/users/{id} with data: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"{apiBaseUrl}/api/users/{id}", content);

                _logger.LogInformation($"Received response: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Success response: {responseJson}");
                    TempData["SuccessMessage"] = "Данные пользователя успешно обновлены";
                    return RedirectToPage(new { id });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error response ({response.StatusCode}): {errorJson}");
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка обновления данных";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка обновления данных";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка обновления данных";
                    }
                    await LoadUserData(id);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error updating user: {ex.Message}");
                await LoadUserData(id);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(int id)
        {
            _logger.LogInformation($"OnPostChangePasswordAsync called for user {id}");

            // Валидируем только поля пароля
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(PasswordInput);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(PasswordInput, validationContext, validationResults, true);

            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ModelState.AddModelError($"PasswordInput.{memberName}", validationResult.ErrorMessage ?? "Ошибка валидации");
                    }
                }
                await LoadUserData(id);
                return Page();
            }

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var currentUserId = _tokenService.GetUserIdFromToken(token);
            if (currentUserId == null || int.Parse(currentUserId) != id)
                return RedirectToPage("/Auth/Login");

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var requestData = new
                {
                    currentPassword = PasswordInput.CurrentPassword,
                    newPassword = PasswordInput.NewPassword,
                    confirmPassword = PasswordInput.ConfirmPassword
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/api/users/{id}/change-password", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Пароль успешно изменен";
                    PasswordInput = new ChangePasswordInputModel(); // Очищаем форму
                    return RedirectToPage(new { id });
                }
                else
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorData = JsonSerializer.Deserialize<JsonElement>(errorJson);
                        if (errorData.TryGetProperty("message", out var messageElement))
                        {
                            ErrorMessage = messageElement.GetString() ?? "Ошибка смены пароля";
                        }
                        else
                        {
                            ErrorMessage = "Ошибка смены пароля";
                        }
                    }
                    catch
                    {
                        ErrorMessage = "Ошибка смены пароля";
                    }
                    await LoadUserData(id);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
                _logger.LogError($"Error changing password: {ex.Message}");
                await LoadUserData(id);
            }

            return Page();
        }

        private async Task LoadUserData(int id)
        {
            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"{apiBaseUrl}/api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    User = JsonSerializer.Deserialize<UserDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading user data: {ex.Message}");
            }
        }
    }
}
