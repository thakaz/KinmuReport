using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KinmuReport.Pages;

[AllowAnonymous]
public class LogoutModel : PageModel
{
    // GETでアクセス → SignOutAsync → /login にリダイレクト
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync("Cookies");
        return Redirect("/login");
    }

}
