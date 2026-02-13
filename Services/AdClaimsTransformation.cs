using System.Security.Claims;
using KinmuReport.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;

namespace KinmuReport.Services;

public class AdClaimsTransformation(
    AttendanceContext context,
    GraphServiceClient graphClient,
    ILogger<AdClaimsTransformation> logger) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var objectId = principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        if (string.IsNullOrEmpty(objectId))
        {
            return principal;
        }

        if (principal.HasClaim(c => c.Type == ClaimNames.EmployeeId))
        {
            return principal;
        }

        // Graph APIでemployeeIdを取得
        var user = await graphClient.Users[objectId]
            .GetAsync(r => r.QueryParameters.Select = ["employeeId", "displayName"]);

        var employeeId = user?.EmployeeId;
        if (string.IsNullOrEmpty(employeeId))
        {
            logger.LogWarning("employeeIdが設定されていません: {ObjectId}", objectId);
            // 社員番号Claimを付与しない → 認可で弾かれる
            return principal;
        }

        // DBで社員を検索、なければ自動登録
        var 社員 = await context.社員s.FirstOrDefaultAsync(e => e.社員番号 == employeeId);
        if (社員 == null)
        {
            社員 = new 社員
            {
                社員番号 = employeeId,
                社員名 = user?.DisplayName ?? "Unknown",
                ログインid = employeeId,
                adオブジェクトid = objectId,
                権限 = Roles.General
            };
            context.社員s.Add(社員);
            await context.SaveChangesAsync();
        }

        var identity = principal.Identity as ClaimsIdentity;
        if (identity != null)
        {
            // 既存のName claimを削除（AD認証のデフォルトはメールアドレス）
            var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
            if (existingNameClaim != null)
            {
                identity.RemoveClaim(existingNameClaim);
            }

            identity.AddClaim(new Claim(ClaimTypes.Name, 社員.社員名));
            identity.AddClaim(new Claim(ClaimNames.EmployeeId, 社員.社員番号));
            identity.AddClaim(new Claim(ClaimTypes.Role, 社員.権限));
            identity.AddClaim(new Claim(ClaimNames.GroupCode, 社員.グループコード ?? ""));
        }

        return principal;
    }
}