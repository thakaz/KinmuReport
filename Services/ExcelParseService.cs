using ClosedXML.Excel;
using KinmuReport.Models;

namespace KinmuReport.Services;

public class ExcelParseService
{
    public (string 社員番号, int 対象年月, List<勤怠> 勤怠リスト) ParseExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);

        //ヘッダー情報を取得
        var headerSheet = workbook.Worksheet("勤務報告書");
        var 社員番号 = headerSheet.Cell("AU1").GetString().Trim();
        var 年 = headerSheet.Cell("AJ1").GetValue<int>();
        var 月 = headerSheet.Cell("AO1").GetValue<int>();

        var 対象年月 = 年 * 100 + 月;

        //勤怠情報を取得
        var dataSheet = workbook.Worksheet("データ");
        var 勤怠リスト = new List<勤怠>();

        for (int row = 2; row <= 32; row++)
        {
            var 勤務日Cell = dataSheet.Cell(row, 1);
            if (勤務日Cell.IsEmpty()) continue;

            var 勤務日 = DateOnly.FromDateTime(勤務日Cell.GetDateTime());
            var record = new 勤怠
            {
                社員番号 = 社員番号,
                対象年月 = 対象年月,
                勤務日 = 勤務日,
                所定内時間 = GetDecimal(dataSheet, row, 5),
                残業時間 = GetDecimal(dataSheet, row, 6),
                深夜残業時間 = GetDecimal(dataSheet, row, 7),
                合計時間 = GetDecimal(dataSheet, row, 8),
                勤怠区分 = dataSheet.Cell(row, 9).IsEmpty() ? null : dataSheet.Cell(row, 9).GetString().Trim(),
                備考 = dataSheet.Cell(row, 10).IsEmpty() ? null : dataSheet.Cell(row, 10).GetString().Trim()
            };

            // 出勤・退勤の time → DateTime 変換
            var startCell = dataSheet.Cell(row, 3);
            var endCell = dataSheet.Cell(row, 4);
            if (!startCell.IsEmpty() && startCell.Value.IsTimeSpan)
            {
                var ts = startCell.GetValue<TimeSpan>();
                var t = TimeOnly.FromTimeSpan(new TimeSpan(ts.Hours % 24, ts.Minutes, ts.Seconds));
                record.出勤日時 = 勤務日.ToDateTime(t);
            }
            if (!endCell.IsEmpty() && endCell.Value.IsTimeSpan)
            {
                var ts = endCell.GetValue<TimeSpan>();
                var t = TimeOnly.FromTimeSpan(new TimeSpan(ts.Hours % 24, ts.Minutes, ts.Seconds));
                record.退勤日時 = 勤務日.ToDateTime(t);

                if (record.出勤日時.HasValue && record.退勤日時 < record.出勤日時)
                    record.退勤日時 = record.退勤日時.Value.AddDays(1);
            }

            勤怠リスト.Add(record);
        }

        return (社員番号, 対象年月, 勤怠リスト);
    }

    private static decimal? GetDecimal(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty() || !cell.Value.IsNumber) return null;
        return cell.GetValue<decimal>();
    }
}
