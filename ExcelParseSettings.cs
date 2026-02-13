namespace KinmuReport;

/// <summary>
/// Excelパース設定
/// </summary>
public class ExcelParseSettings
{
    /// <summary>ヘッダー情報のシート名</summary>
    public string HeaderSheetName { get; set; } = "勤務報告書";

    /// <summary>データのシート名</summary>
    public string DataSheetName { get; set; } = "データ";

    /// <summary>ヘッダー情報のセル位置</summary>
    public HeaderCells Header { get; set; } = new();

    /// <summary>データの列位置（1始まり）</summary>
    public DataColumns Data { get; set; } = new();

    /// <summary>データの行範囲</summary>
    public int DataStartRow { get; set; } = 2;
    public int DataEndRow { get; set; } = 32;

    public class HeaderCells
    {
        public string 社員番号 { get; set; } = "AU1";
        public string 年 { get; set; } = "AJ1";
        public string 月 { get; set; } = "AO1";
    }

    public class DataColumns
    {
        public int 勤務日 { get; set; } = 1;
        public int 曜日 { get; set; } = 2;
        public int 出勤 { get; set; } = 3;
        public int 退勤 { get; set; } = 4;
        public int 所定内 { get; set; } = 5;
        public int 残業 { get; set; } = 6;
        public int 深夜 { get; set; } = 7;
        public int 合計 { get; set; } = 8;
        public int 勤怠区分 { get; set; } = 9;
        public int 備考 { get; set; } = 10;
    }
}
