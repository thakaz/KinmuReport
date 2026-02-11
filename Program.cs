using Microsoft.EntityFrameworkCore;
using KinmuReport.Components;

namespace KinmuReport;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        //ログイン用のRazorPagesを追加
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<KinmuReport.Models.AttendanceContext>(
            options => options.UseNpgsql(
                builder.Configuration.GetConnectionString("AttendanceContext") ??
                "Host=localhost;Port=5433;Database=attendance;Username=postgres;Password=postgres"
            ));


        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
            });


        builder.Services.AddAuthorization();

        builder.Services.AddCascadingAuthenticationState();


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
