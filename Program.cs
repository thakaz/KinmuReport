using KinmuReport.Components;
using KinmuReport.Services;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Blazor
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB（Excel アップロード用）
            });

        // Razor Pages（ログイン・ログアウト用）
        builder.Services.AddRazorPages();

        // DB
        builder.Services.AddDbContext<KinmuReport.Models.AttendanceContext>(
            options => options.UseNpgsql(
                builder.Configuration.GetConnectionString("AttendanceContext") ??
                "Host=localhost;Port=5433;Database=attendance;Username=postgres;Password=postgres"
            ));

        // 認証・認可
        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            });
        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();

        // アプリケーションサービス
        builder.Services.AddScoped<ExcelParseService>();
        builder.Services.AddScoped<LockService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorPages(); //ログイン用のRazorPagesをマッピング

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
