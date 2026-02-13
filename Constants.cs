namespace KinmuReport;

/// <summary>
/// Claim名の定数
/// </summary>
public static class ClaimNames
{
    public const string EmployeeId = "社員番号";
    public const string GroupCode = "グループコード";
}

/// <summary>
/// 権限（ロール）の定数
/// </summary>
public static class Roles
{
    public const string Admin = "管理者";
    public const string GroupAdmin = "グループ管理者";
    public const string General = "一般";
}

/// <summary>
/// アプリケーション設定の定数
/// </summary>
public static class AppConstants
{
    /// <summary>ロックのタイムアウト期間</summary>
    public static readonly TimeSpan LockTimeout = TimeSpan.FromDays(1);

    /// <summary>アップロード最大サイズ（バイト）</summary>
    public const int MaxUploadSizeBytes = 10 * 1024 * 1024;

    /// <summary>年度開始月（4月）</summary>
    public const int FiscalYearStartMonth = 4;

    /// <summary>SharePointのルートフォルダ</summary>
    public const string SharePointRootFolder = "/勤務報告";
}

/// <summary>
/// 年月（YYYYMM形式）の拡張メソッド
/// </summary>
public static class YearMonthExtensions
{
    /// <summary>DateTimeからYYYYMM形式のintに変換</summary>
    public static int ToYearMonth(this DateTime date) => date.Year * 100 + date.Month;

    /// <summary>DateOnlyからYYYYMM形式のintに変換</summary>
    public static int ToYearMonth(this DateOnly date) => date.Year * 100 + date.Month;

    /// <summary>YYYYMM形式から年を取得</summary>
    public static int GetYear(this int yearMonth) => yearMonth / 100;

    /// <summary>YYYYMM形式から月を取得</summary>
    public static int GetMonth(this int yearMonth) => yearMonth % 100;

    /// <summary>YYYYMM形式からDateTimeに変換</summary>
    public static DateTime ToDateTime(this int yearMonth) => new(yearMonth.GetYear(), yearMonth.GetMonth(), 1);
}

/// <summary>
/// 年月関連のユーティリティ（拡張メソッドにできないもの）
/// </summary>
public static class YearMonthHelper
{
    /// <summary>年と月からYYYYMM形式のintに変換</summary>
    public static int ToYearMonth(int year, int month) => year * 100 + month;

    /// <summary>現在の年度を取得（4月始まり）</summary>
    public static int GetCurrentFiscalYear()
    {
        var now = DateTime.Now;
        return now.Month >= AppConstants.FiscalYearStartMonth ? now.Year : now.Year - 1;
    }
}
