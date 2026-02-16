namespace KinmuReport;

public class CommuteParseSettings
{
    public string SheetName { get; set; } = "通勤手当申請";

    public HeaderCells Header { get; set; } = new();
    public DataColumns DataLeft { get; set; } = new();
    public DataColumns DataRight { get; set; } = new();

    public int DataStartRow { get; set; } = 31;
    public int DataEndRow { get; set; } = 46;

    public class HeaderCells
    {
        public string 年 { get; set; } = "AM1";
        public string 月 { get; set; } = "AS1";
        public string 社員番号 { get; set; } = "AX1";
    }

    public class DataColumns
    {
        public int 日付 { get; set; }
        public int 経路NO { get; set; }
        public int 金額 { get; set; }
        public int 備考 { get; set; }
    }
}
