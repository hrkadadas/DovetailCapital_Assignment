using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using FileGenerationUIApp.Models;

namespace FileGenerationUIApp.Pages.Masters
{
    public class DesignationModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        public List<Dictionary<string, string>> Designations { get; set; } = new();

        [BindProperty]
        public string? NewTitle { get; set; }

        [BindProperty]
        public string? NewDescription { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public DesignationModel(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _factory.CreateClient("ApiClient");
                var resp = await client.GetAsync("api/Masters/designations");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var apiResp = JsonSerializer.Deserialize<ApiResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (apiResp?.Success == true && apiResp.Data is JsonElement el && el.ValueKind == JsonValueKind.Array)
                    {
                        Designations = el.EnumerateArray().Select(item =>
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
            if (string.IsNullOrWhiteSpace(NewTitle))
            {
                StatusMessage = "Title is required.";
                return RedirectToPage();
            }

            try
            {
                var client = _factory.CreateClient("ApiClient");
                var payload = new { Id = 0, Name = NewTitle };
                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("api/Masters/designations", content);
                if (resp.IsSuccessStatusCode)
                {
                    StatusMessage = "Designation created.";
                }
                else
                {
                    StatusMessage = $"Failed to create designation: {resp.StatusCode}";
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
