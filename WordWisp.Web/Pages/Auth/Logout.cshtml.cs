using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WordWisp.Web.Pages.Auth  
{
    public class LogoutModel : PageModel  
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LogoutModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5118";

                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    await httpClient.PostAsync($"{apiBaseUrl}/api/auth/logout", null);
                }
                catch
                {
                    // Игнорируем ошибки API при logout
                }
                Response.Cookies.Delete("AuthToken");
            }

            return Redirect("/");
        }
    }
}
