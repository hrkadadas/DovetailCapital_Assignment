using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FileGenerationUIApp.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (Request.Cookies.ContainsKey("ApiToken"))
            {
                Response.Cookies.Delete("ApiToken");
            }

            return RedirectToPage("/Index");
        }
    }
}
