using ClosedXML.Excel;
using KinmuReport.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KinmuReport.Services;

public enum BulkFileType { Attendance, Commute, Unknown }

public enum BulkUploadStatus
{
    Pending,
    Parsed,
    Saving,
    Completed,
    Error
}

public class BulkUploadItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = "";
    public byte[] FileBytes { get; set; } = [];
    public BulkFileType FileType { get; set; } = BulkFileType.Unknown;
    public BulkUploadStatus Status { get; set; } = BulkUploadStatus.Pending;
    public string? ErrorMessage { get; set; }

    // パース結果（勤務報告）
    public (string 社員番号, int 対象年月, List<勤怠> 勤怠リスト)? AttendanceResult { get; set; }

    // パース結果（通勤手当）- 社員名を追加
    public (string 社員番号, string? 社員名, int 対象年月, List<通勤手当> 通勤手当リスト)? CommuteResult { get; set; }

    // Excelから取得した社員名（逆引き用）
    public string? Excel社員名 { get; set; }

    // 社員情報
    public string? 社員番号 => AttendanceResult?.社員番号 ?? CommuteResult?.社員番号;
    public int? 対象年月 => AttendanceResult?.対象年月 ?? CommuteResult?.対象年月;
    public string? 社員名 { get; set; }

    // 権限チェック結果
    public bool IsAuthorized { get; set; }
    public string? AuthErrorMessage { get; set; }

    // ロックチェック結果（勤務報告のみ）
    public bool IsLocked { get; set; }
    public string? LockedBy { get; set; }
}

public class BulkUploadService
{
    private readonly ExcelParseService _attendanceParser;
    private readonly CommuteParseService _commuteParser;
    private readonly IOptions<ExcelParseSettings> _attendanceSettings;
    private readonly IOptions<CommuteParseSettings> _commuteSettings;

    public BulkUploadService(
        ExcelParseService attendanceParser,
        CommuteParseService commuteParser,
        IOptions<ExcelParseSettings> attendanceSettings,
        IOptions<CommuteParseSettings> commuteSettings)
    {
        _attendanceParser = attendanceParser;
        _commuteParser = commuteParser;
        _attendanceSettings = attendanceSettings;
        _commuteSettings = commuteSettings;
    }

    /// <summary>
    /// ファイル種別を自動判定する
    /// </summary>
    public BulkFileType DetectFileType(Stream stream)
    {
        try
        {
            using var workbook = new XLWorkbook(stream);

            // 勤務報告: "勤務報告書" シートが存在
            if (workbook.Worksheets.Any(ws => ws.Name == _attendanceSettings.Value.HeaderSheetName))
                return BulkFileType.Attendance;

            // 通勤手当: "通勤手当申請" シートが存在
            if (workbook.Worksheets.Any(ws => ws.Name == _commuteSettings.Value.SheetName))
                return BulkFileType.Commute;

            return BulkFileType.Unknown;
        }
        catch
        {
            return BulkFileType.Unknown;
        }
    }

    /// <summary>
    /// ファイルをパースしてBulkUploadItemを作成する
    /// </summary>
    public BulkUploadItem ParseFile(byte[] fileBytes, string fileName)
    {
        var item = new BulkUploadItem
        {
            FileName = fileName,
            FileBytes = fileBytes
        };

        try
        {
            using var ms = new MemoryStream(fileBytes);
            item.FileType = DetectFileType(ms);
            ms.Position = 0;

            switch (item.FileType)
            {
                case BulkFileType.Attendance:
                    item.AttendanceResult = _attendanceParser.ParseExcel(ms);
                    item.Status = BulkUploadStatus.Parsed;
                    break;

                case BulkFileType.Commute:
                    // 社員番号がなくてもパースできるように、例外をキャッチ
                    try
                    {
                        var result = _commuteParser.ParseExcel(ms);
                        item.CommuteResult = result;
                        item.Excel社員名 = result.社員名;
                        item.Status = BulkUploadStatus.Parsed;
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("社員番号"))
                    {
                        // 社員番号がない場合、社員名だけ取得してパース結果は後で設定
                        ms.Position = 0;
                        item.Excel社員名 = ExtractEmployeeName(ms);
                        item.Status = BulkUploadStatus.Parsed;
                        // CommuteResultはnullのまま、後で社員番号を逆引きしてから再パース
                    }
                    break;

                default:
                    item.Status = BulkUploadStatus.Error;
                    item.ErrorMessage = "ファイル形式を判定できません";
                    break;
            }
        }
        catch (Exception ex)
        {
            item.Status = BulkUploadStatus.Error;
            item.ErrorMessage = $"パースエラー: {ex.Message}";
        }

        return item;
    }

    /// <summary>
    /// 通勤手当Excelから社員名だけを抽出
    /// </summary>
    private string? ExtractEmployeeName(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(_commuteSettings.Value.SheetName);
        var cell = sheet.Cell(_commuteSettings.Value.Header.社員名);
        if (cell.IsEmpty()) return null;
        return NormalizeString(cell.GetString());
    }

    /// <summary>
    /// 権限チェック（グループ内社員かどうか）
    /// 社員番号がない場合は社員名から逆引きする
    /// </summary>
    public async Task CheckAuthorizationAsync(
        BulkUploadItem item,
        AttendanceContext context,
        string currentRole,
        string currentGroup)
    {
        // 通勤手当で社員番号がない場合、社員名から逆引き
        if (item.FileType == BulkFileType.Commute && item.CommuteResult == null && item.Excel社員名 != null)
        {
            var resolvedEmployee = await FindEmployeeByNameAsync(context, item.Excel社員名, currentRole, currentGroup);
            if (resolvedEmployee == null)
            {
                item.IsAuthorized = false;
                item.AuthErrorMessage = $"社員名「{item.Excel社員名}」が見つかりません";
                return;
            }

            // 社員番号が見つかったので、再パース
            using var ms = new MemoryStream(item.FileBytes);
            var result = _commuteParser.ParseExcel(ms, resolvedEmployee.社員番号);
            item.CommuteResult = result;
            item.社員名 = resolvedEmployee.社員名;
        }

        if (item.社員番号 == null)
        {
            item.IsAuthorized = false;
            item.AuthErrorMessage = "社員番号が取得できません";
            return;
        }

        // 管理者は全員OK
        if (currentRole == Roles.Admin)
        {
            var emp = await context.社員s.FirstOrDefaultAsync(e => e.社員番号 == item.社員番号);
            item.社員名 = emp?.社員名;
            item.IsAuthorized = emp != null;
            if (!item.IsAuthorized)
                item.AuthErrorMessage = "社員が見つかりません";
            return;
        }

        // グループ管理者は自グループの社員のみ
        if (currentRole == Roles.GroupAdmin)
        {
            var emp = await context.社員s.FirstOrDefaultAsync(e => e.社員番号 == item.社員番号);
            item.社員名 = emp?.社員名;

            if (emp == null)
            {
                item.IsAuthorized = false;
                item.AuthErrorMessage = "社員が見つかりません";
                return;
            }

            if (emp.グループコード != currentGroup)
            {
                item.IsAuthorized = false;
                item.AuthErrorMessage = "グループ外の社員です";
                return;
            }

            item.IsAuthorized = true;
            return;
        }

        // それ以外は不許可
        item.IsAuthorized = false;
        item.AuthErrorMessage = "権限がありません";
    }

    /// <summary>
    /// 社員名から社員を検索
    /// </summary>
    private async Task<社員?> FindEmployeeByNameAsync(
        AttendanceContext context,
        string employeeName,
        string currentRole,
        string currentGroup)
    {
        // 検索対象の社員リストを取得
        IQueryable<社員> query = context.社員s;

        // グループ管理者は自グループのみ
        if (currentRole == Roles.GroupAdmin)
        {
            query = query.Where(e => e.グループコード == currentGroup);
        }

        var employees = await query.ToListAsync();

        // 正規化した名前で比較
        var normalizedSearchName = NormalizeForComparison(employeeName);

        foreach (var emp in employees)
        {
            var normalizedEmpName = NormalizeForComparison(emp.社員名);
            if (normalizedSearchName == normalizedEmpName)
            {
                return emp;
            }
        }

        return null;
    }

    /// <summary>
    /// 文字列を正規化（トリム、全角半角、スペース除去）
    /// </summary>
    private static string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        return input
            .Replace('　', ' ')  // 全角スペース→半角
            .Replace(" ", "")    // スペース除去
            .Trim();
    }

    /// <summary>
    /// 比較用に正規化（トリム、スペース除去）
    /// </summary>
    private static string NormalizeForComparison(string input)
    {
        return NormalizeString(input);
    }

    /// <summary>
    /// ロックチェック（勤務報告のみ）
    /// </summary>
    public async Task CheckLockAsync(
        BulkUploadItem item,
        LockService lockService,
        string currentUserId)
    {
        if (item.FileType != BulkFileType.Attendance || item.AttendanceResult == null)
            return;

        var (社員番号, 対象年月, _) = item.AttendanceResult.Value;
        var lockRecord = await lockService.GetLock(社員番号, 対象年月);

        if (lockRecord != null && lockRecord.ロック者番号 != currentUserId)
        {
            item.IsLocked = true;
            item.LockedBy = lockRecord.ロック者番号Navigation?.社員名 ?? lockRecord.ロック者番号;
        }
    }
}
