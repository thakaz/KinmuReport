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

        builder.Services.AddSingleton<SharePointService>();

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

        //ダウンロード用のAPI
        app.MapGet("/api/download/{社員番号}/{対象年月:int}", 
            async (string 社員番号, int 対象年月, SharePointService sharePoint,LockService lockService,HttpContext httpContext) =>
        {
            //認証チェック
            if(!httpContext.User.Identity?.IsAuthenticated ?? true)
            {
                return Results.Unauthorized();
            }

            var currentUserId = httpContext.User.FindFirst("社員番号")?.Value ??"";

            //ロック取得
            var acquired = await lockService.TryAcquire(社員番号, 対象年月, currentUserId);
            if(!acquired)
            {
                var existing = await lockService.GetLock(社員番号, 対象年月);
                return Results.Conflict($"{existing?.ロック者番号Navigation}が編集中のため、ダウンロードできません。");
            }


            var folderPath = $"/勤務報告/{社員番号}";
            var fileName = $"{対象年月}_{社員番号}_勤務報告.xlsm";
            var stream = await sharePoint.DownloadAsync(folderPath, fileName);
            if (stream == null)
            {
                return Results.NotFound("ファイルが見つかりません");
            }
            return Results.File(stream, "application/vnd.ms-excel.sheet.macroEnabled.12", fileName);
        }).RequireAuthorization();


        app.Run();
    }
}
