using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using KinmuReport.Models;

namespace KinmuReport.Services;

public class LockService(AttendanceContext context, IOptions<AppSettings> appSettingsOptions)
{
    private AppSettings AppSettings => appSettingsOptions.Value;

    /// ロック状態を取得（タイムアウト済みは自動削除）
    public async Task<ロック?> GetLock(string 社員番号, int 対象年月)
    {
        var lockRecord = await context.ロックs
            .Include(l => l.ロック者番号Navigation)
            .FirstOrDefaultAsync(l => l.社員番号 == 社員番号 && l.対象年月 == 対象年月);

        if (lockRecord == null)
        {
            return null;
        }

        // タイムアウトチェック
        if (DateTime.Now - lockRecord.ロック日時 > AppSettings.LockTimeout)
        {
            context.ロックs.Remove(lockRecord);
            await context.SaveChangesAsync();
            return null;
        }

        return lockRecord;
    }

    /// ロック取得（成功: true、既にロック中: false）
    public async Task<bool> TryAcquire(string 社員番号, int 対象年月, string ロック者番号)
    {
        // 期限切れロックを先にクリーンアップ
        var existing = await context.ロックs
            .FirstOrDefaultAsync(l => l.社員番号 == 社員番号 && l.対象年月 == 対象年月);

        if (existing != null)
        {
            if (DateTime.Now - existing.ロック日時 > AppSettings.LockTimeout)
            {
                context.ロックs.Remove(existing);
                await context.SaveChangesAsync();
            }
            else
            {
                return false; // 有効なロックが存在
            }
        }

        // INSERT を試みて、重複なら例外をキャッチ（レースコンディション対策）
        try
        {
            context.ロックs.Add(new ロック
            {
                社員番号 = 社員番号,
                対象年月 = 対象年月,
                ロック者番号 = ロック者番号,
                ロック日時 = DateTime.Now
            });
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            // 主キー違反 = 他のユーザーが先にロック取得
            return false;
        }
    }

    /// ロック解除（本人 or 管理者）
    public async Task<bool> Release(string 社員番号, int 対象年月, string 操作者番号, string 操作者ロール)
    {
        var lockRecord = await context.ロックs
            .FirstOrDefaultAsync(l => l.社員番号 == 社員番号 && l.対象年月 == 対象年月);

        if (lockRecord == null)
        {
            return false;
        }

        // 本人または管理者のみ解除可能
        if (lockRecord.ロック者番号 != 操作者番号 && 操作者ロール != Roles.Admin)
        {
            return false;
        }

        context.ロックs.Remove(lockRecord);
        await context.SaveChangesAsync();
        return true;
    }
}
