using FileGenerationUIApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FileGenerationUIApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpFactory;

        public LoginModel(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpFactory.CreateClient("ApiClient");

            var payload = new
            {
                Username = Username,
                Password = Password
            };

            var resp = await client.PostAsJsonAsync("api/auth/login", payload);

            if (!resp.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var result = await resp.Content.ReadFromJsonAsync<ApiResponse>();

            if (result == null || !result.Success)
            {
                ErrorMessage = result?.Message ?? "Invalid username or password.";
                return Page();
            }

            if (result.Data is JsonElement dataElement &&
                dataElement.TryGetProperty("token", out var tokenElement))
            {
                var token = tokenElement.GetString();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    HttpContext.Response.Cookies.Append(
                        "ApiToken",
                        token,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });

                    return RedirectToPage("/Dashboard/Index");
                }
            }

            ErrorMessage = "Unexpected response from server.";
            return Page();
        }
    }
}