using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.Auth;

namespace WordWisp.Web.Pages.Auth
{
    public class VerifyEmailModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public VerifyEmailModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public VerifyEmailInputModel Input { get; set; } = new();

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            if (TempData.ContainsKey("Email"))
            {
                Input.Email = TempData["Email"]?.ToString() ?? "";
            }

            if (TempData.ContainsKey("SuccessMessage"))
            {
                Message = TempData["SuccessMessage"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostVerifyAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                var verifyRequest = new
                {
                    email = Input.Email,
                    verificationCode = Input.VerificationCode
                };

                var json = JsonSerializer.Serialize(verifyRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/auth/verify-email", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Email успешно подтвержден! Теперь вы можете войти в систему.";
                    TempData["Email"] = Input.Email;

                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                    ErrorMessage = errorResponse?.Message ?? "Неверный код подтверждения";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendCodeAsync()
        {
            if (string.IsNullOrEmpty(Input.Email))
            {
                ErrorMessage = "Введите email для повторной отправки кода";
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                var json = JsonSerializer.Serialize(Input.Email);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/auth/resend-verification", content);

                if (response.IsSuccessStatusCode)
                {
                    Message = "Код подтверждения отправлен повторно на вашу почту";
                    Input.VerificationCode = "";
                    ErrorMessage = null;
                }
                else
                {
                    ErrorMessage = "Не удалось отправить код повторно";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Не удалось подключиться к серверу";
            }

            return Page();
        }
    }
}
