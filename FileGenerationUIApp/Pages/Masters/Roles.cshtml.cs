using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using FileGenerationUIApp.Models;

namespace FileGenerationUIApp.Pages.Masters
{
    public class RolesModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public List<Dictionary<string, string>> Roles { get; set; } = new();

        [BindProperty]
        public string? NewRoleName { get; set; }

        [BindProperty]
        public string? NewRoleDescription { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public RolesModel(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _factory.CreateClient("ApiClient");
                var resp = await client.GetAsync("api/Masters/roles");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var apiResp = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResp?.Success == true && apiResp.Data is JsonElement el && el.ValueKind == JsonValueKind.Array)
                    {
                        Roles = el.EnumerateArray().Select(item =>
                        {
                            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var prop in item.EnumerateObject()) dict[prop.Name] = prop.Value.ToString();
                            return dict;
                        }).ToList();
                    }
                }
            }
            catch
            {
                // ignore errors for prototype
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(NewRoleName))
            {
                StatusMessage = "Name is required.";
                return RedirectToPage();
            }

            try
            {
                var client = _factory.CreateClient("ApiClient");
                var payload = new { Id = 0, Name = NewRoleName };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("api/Masters/roles", content);
                if (resp.IsSuccessStatusCode)
                {
                    StatusMessage = "Role created.";
                }
                else
                {
                    StatusMessage = $"Failed to create role: {resp.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
