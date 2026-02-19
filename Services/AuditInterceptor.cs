using KinmuReport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KinmuReport.Services;

public class AuditInterceptor :SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData
        , InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if(eventData.Context is AttendanceContext context)
        {
            OnBeforeSaveChanges(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void OnBeforeSaveChanges(AttendanceContext context)
    {
        context.ChangeTracker.DetectChanges(); //変更を検出してからエントリを取得する

        foreach(var entry in context.ChangeTracker.Entries().ToList())
        {
            //監査ログ自体は記録しない
            if(entry.Entity is 監査ログ)
            {
                continue;
            }

            //UnchangedやDeatachedは無視する
            if(entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
            {
                continue;
            }

            var log = new 監査ログ
            {
                操作種別 = entry.State.ToString(),
                テーブル名 = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                レコードキー = GetPrimaryKeyValue(entry),
                変更内容 = GetChanges(entry),
                操作者 = GetCurrentUser()
            };
            context.監査ログs.Add(log);
        }
    }

    private string GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties == null) return "";

        var keyValues = keyProperties.Select(p=>$"{p.Name}={entry.Property(p.Name).CurrentValue}").ToArray();

        return string.Join(",", keyValues);
    }

    private  string? GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {

        if (entry.State == EntityState.Added) {
            return null; //新規追加は変更前がないため、変更内容は記録しない
        }

        var changes = new Dictionary<string, object?>();

        foreach (var prop in entry.OriginalValues.Properties)
        {
            var original = entry.OriginalValues[prop];
            var current = entry.CurrentValues[prop];

            if(!Equals(original, current))
            {
                changes[prop.Name] = new {before = original, after = current};
            }

        }
        return changes.Count > 0
            ? System.Text.Json.JsonSerializer.Serialize(changes,new System.Text.Json.JsonSerializerOptions
            {
                Encoder =System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, //日本語をエスケープしない
            })
            : null;
    }

    private string? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimNames.EmployeeId)?.Value;
    }


}
