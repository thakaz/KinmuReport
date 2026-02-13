using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using KinmuReport.Models;

namespace KinmuReport.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly AttendanceContext _context;

    public string? ErrorMessage { get; set; }

    public LoginModel(AttendanceContext context)
    {
        _context = context;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string loginId, string password)
    {
        var 社員 = _context.社員s
            .FirstOrDefault(e => e.ログインid == loginId);

        if (社員 == null || !BCrypt.Net.BCrypt.Verify(password, 社員.パスワードハッシュ))
        {
            ErrorMessage = "ログインIDまたはパスワードが正しくありません。";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, 社員.社員名),
            new(ClaimNames.EmployeeId, 社員.社員番号),
            new(ClaimTypes.Role, 社員.権限),
            new(ClaimNames.GroupCode, 社員.グループコード ?? "")
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        return Redirect("/");
    }
}
