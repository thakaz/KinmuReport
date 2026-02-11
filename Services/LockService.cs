using Microsoft.EntityFrameworkCore;
using KinmuReport.Models;

namespace KinmuReport.Services;

public class LockService(AttendanceContext context)
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromDays(1);

    /// ロック状態を取得（タイムアウト済みは自動削除）
    public async Task<ロック?> GetLock(string 社員番号, int 対象年月)
    {
        var lockRecord = await context.ロックs
            .Include(l => l.ロック者番号Navigation)
            .FirstOrDefaultAsync(l => l.社員番号 == 社員番号 && l.対象年月 == 対象年月);

        if (lockRecord == null) return null;

        // タイムアウトチェック
        if (DateTime.Now - lockRecord.ロック日時 > LockTimeout)
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
        var existing = await GetLock(社員番号, 対象年月);
        if (existing != null) return false;

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

    /// ロック解除（本人 or 管理者）
    public async Task<bool> Release(string 社員番号, int 対象年月, string 操作者番号, string 操作者ロール)
    {
        var lockRecord = await context.ロックs
            .FirstOrDefaultAsync(l => l.社員番号 == 社員番号 && l.対象年月 == 対象年月);

        if (lockRecord == null) return false;

        // 本人または管理者のみ解除可能
        if (lockRecord.ロック者番号 != 操作者番号 && 操作者ロール != "管理者")
            return false;

        context.ロックs.Remove(lockRecord);
        await context.SaveChangesAsync();
        return true;
    }
}
