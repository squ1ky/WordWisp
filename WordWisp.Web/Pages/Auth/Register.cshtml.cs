using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WordWisp.Web.Models.Auth;

namespace WordWisp.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public RegisterInputModel Input { get; set; } = new();

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                var registerRequest = new
                {
                    username = Input.Username,
                    name = Input.Name,
                    surname = Input.Surname,
                    email = Input.Email,
                    password = Input.Password,
                    role = Input.Role
                };

                var json = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{apiBaseUrl}/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Email"] = Input.Email;
                    TempData["SuccessMessage"] = "Регистрация успешна! Проверьте почту для подтверждения email.";

                    return RedirectToPage("/Auth/VerifyEmail");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorContent);
                    ErrorMessage = errorResponse?.Message ?? "Произошла ошибка при регистрации";
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
