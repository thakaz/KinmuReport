namespace KinmuReport;

/// <summary>
/// アプリケーション設定（appsettings.jsonから読み込み）
/// </summary>
public class AppSettings
{
    /// <summary>締め日（1〜28、または0で月末）</summary>
    public int ClosingDay { get; set; } = 15;

    /// <summary>年度開始月（4月）</summary>
    public int FiscalYearStartMonth { get; set; } = 4;

    /// <summary>ロックのタイムアウト時間（時間単位）</summary>
    public int LockTimeoutHours { get; set; } = 24;

    /// <summary>SharePointのルートフォルダ</summary>
    public string SharePointRootFolder { get; set; } = "/勤務報告";

    /// <summary>アップロード最大サイズ（MB）</summary>
    public int MaxUploadSizeMB { get; set; } = 10;

    // 計算プロパティ
    public TimeSpan LockTimeout => TimeSpan.FromHours(LockTimeoutHours);
    public int MaxUploadSizeBytes => MaxUploadSizeMB * 1024 * 1024;

    /// <summary>締め日の表示文字列</summary>
    public string ClosingDayLabel => ClosingDay == 0 ? "月末締め" : $"{ClosingDay}日締め";
}
