#r "nuget: ClosedXML, 0.104.2"

using ClosedXML.Excel;

// サンプルデータ
var 社員番号 = "E001";
var 年 = 2025;
var 月 = 2;

var workbook = new XLWorkbook();

// ========== 勤務報告書シート ==========
var headerSheet = workbook.AddWorksheet("勤務報告書");
headerSheet.Cell("AJ1").Value = 年;
headerSheet.Cell("AO1").Value = 月;
headerSheet.Cell("AU1").Value = 社員番号;

// 見やすくするためのラベル
headerSheet.Cell("AI1").Value = "年:";
headerSheet.Cell("AN1").Value = "月:";
headerSheet.Cell("AT1").Value = "社員番号:";

// ========== データシート ==========
var dataSheet = workbook.AddWorksheet("データ");

// ヘッダー行
dataSheet.Cell(1, 1).Value = "日";
dataSheet.Cell(1, 2).Value = "曜日";
dataSheet.Cell(1, 3).Value = "開始";
dataSheet.Cell(1, 4).Value = "終了";
dataSheet.Cell(1, 5).Value = "所定内";
dataSheet.Cell(1, 6).Value = "残業";
dataSheet.Cell(1, 7).Value = "深夜残業";
dataSheet.Cell(1, 8).Value = "合計";
dataSheet.Cell(1, 9).Value = "勤怠区分";
dataSheet.Cell(1, 10).Value = "備考";

// 15日締め（前月16日〜当月15日）のサンプルデータ
var startDate = new DateTime(年, 月, 1).AddMonths(-1).AddDays(15); // 前月16日
var endDate = new DateTime(年, 月, 15);

int row = 2;
for (var date = startDate; date <= endDate; date = date.AddDays(1))
{
    var dayOfWeek = (int)date.DayOfWeek; // 0=日, 1=月, ..., 6=土
    var isWeekend = dayOfWeek == 0 || dayOfWeek == 6;

    dataSheet.Cell(row, 1).Value = date;
    dataSheet.Cell(row, 1).Style.NumberFormat.Format = "yyyy/mm/dd";

    dataSheet.Cell(row, 2).Value = dayOfWeek + 1; // 1=日, 2=月, ..., 7=土

    if (!isWeekend)
    {
        // 平日のサンプル
        dataSheet.Cell(row, 3).Value = new TimeSpan(9, 0, 0);  // 09:00
        dataSheet.Cell(row, 3).Style.NumberFormat.Format = "h:mm";

        dataSheet.Cell(row, 4).Value = new TimeSpan(18, 0, 0); // 18:00
        dataSheet.Cell(row, 4).Style.NumberFormat.Format = "h:mm";

        dataSheet.Cell(row, 5).Value = 8.0m;  // 所定内
        dataSheet.Cell(row, 6).Value = 0.0m;  // 残業
        dataSheet.Cell(row, 7).Value = 0.0m;  // 深夜
        dataSheet.Cell(row, 8).Value = 8.0m;  // 合計
    }
    else
    {
        // 土日
        dataSheet.Cell(row, 9).Value = "休日";
    }

    row++;
}

// 残業日のサンプル（3行目を上書き）
dataSheet.Cell(3, 4).Value = new TimeSpan(21, 0, 0); // 21:00まで
dataSheet.Cell(3, 5).Value = 8.0m;
dataSheet.Cell(3, 6).Value = 3.0m;  // 残業3時間
dataSheet.Cell(3, 7).Value = 0.0m;
dataSheet.Cell(3, 8).Value = 11.0m;
dataSheet.Cell(3, 10).Value = "客先打ち合わせ";

// 深夜残業のサンプル（5行目を上書き）
dataSheet.Cell(5, 4).Value = new TimeSpan(23, 30, 0); // 23:30まで
dataSheet.Cell(5, 5).Value = 8.0m;
dataSheet.Cell(5, 6).Value = 3.5m;
dataSheet.Cell(5, 7).Value = 1.5m;  // 深夜1.5時間
dataSheet.Cell(5, 8).Value = 13.0m;
dataSheet.Cell(5, 10).Value = "リリース作業";

// 有給のサンプル（10行目を上書き）
dataSheet.Cell(10, 3).Value = "";
dataSheet.Cell(10, 4).Value = "";
dataSheet.Cell(10, 5).Value = 0.0m;
dataSheet.Cell(10, 6).Value = 0.0m;
dataSheet.Cell(10, 7).Value = 0.0m;
dataSheet.Cell(10, 8).Value = 0.0m;
dataSheet.Cell(10, 9).Value = "有給休暇";

// 列幅調整
dataSheet.Column(1).Width = 12;
dataSheet.Column(3).Width = 8;
dataSheet.Column(4).Width = 8;
dataSheet.Column(9).Width = 12;
dataSheet.Column(10).Width = 20;

// 保存
var outputPath = Path.Combine(Environment.CurrentDirectory, $"sample_{年}{月:D2}_{社員番号}.xlsx");
workbook.SaveAs(outputPath);

Console.WriteLine($"サンプルExcel生成完了: {outputPath}");
Console.WriteLine();
Console.WriteLine("【勤務報告書シート】");
Console.WriteLine($"  AJ1: {年}（年）");
Console.WriteLine($"  AO1: {月}（月）");
Console.WriteLine($"  AU1: {社員番号}（社員番号）");
Console.WriteLine();
Console.WriteLine("【データシート】");
Console.WriteLine("  A列: 勤務日");
Console.WriteLine("  B列: 曜日（1=日〜7=土）");
Console.WriteLine("  C列: 出勤時刻");
Console.WriteLine("  D列: 退勤時刻");
Console.WriteLine("  E列: 所定内時間");
Console.WriteLine("  F列: 残業時間");
Console.WriteLine("  G列: 深夜残業時間");
Console.WriteLine("  H列: 合計時間");
Console.WriteLine("  I列: 勤怠区分");
Console.WriteLine("  J列: 備考");
