using ClosedXML.Excel;
using KinmuReport.Models;
using Microsoft.Extensions.Options;

namespace KinmuReport.Services;

public class ExcelParseService
{
    private readonly ExcelParseSettings _settings;

    public ExcelParseService(IOptions<ExcelParseSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string 社員番号, int 対象年月, List<勤怠> 勤怠リスト) ParseExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);

        //ヘッダー情報を取得
        var headerSheet = workbook.Worksheet(_settings.HeaderSheetName);
        var 社員番号 = headerSheet.Cell(_settings.Header.社員番号).GetString().Trim();
        var 年 = headerSheet.Cell(_settings.Header.年).GetValue<int>();
        var 月 = headerSheet.Cell(_settings.Header.月).GetValue<int>();

        var 対象年月 = 年 * 100 + 月;

        //勤怠情報を取得
        var dataSheet = workbook.Worksheet(_settings.DataSheetName);
        var 勤怠リスト = new List<勤怠>();
        var cols = _settings.Data;

        for (int row = _settings.DataStartRow; row <= _settings.DataEndRow; row++)
        {
            var 勤務日Cell = dataSheet.Cell(row, cols.勤務日);
            if (勤務日Cell.IsEmpty()) continue;

            var 勤務日 = DateOnly.FromDateTime(勤務日Cell.GetDateTime());
            var record = new 勤怠
            {
                社員番号 = 社員番号,
                対象年月 = 対象年月,
                勤務日 = 勤務日,
                所定内時間 = GetDecimal(dataSheet, row, cols.所定内),
                残業時間 = GetDecimal(dataSheet, row, cols.残業),
                深夜残業時間 = GetDecimal(dataSheet, row, cols.深夜),
                合計時間 = GetDecimal(dataSheet, row, cols.合計),
                勤怠区分 = GetString(dataSheet, row, cols.勤怠区分),
                備考 = GetString(dataSheet, row, cols.備考)
            };

            // 出勤・退勤の time → DateTime 変換
            var startCell = dataSheet.Cell(row, cols.出勤);
            var endCell = dataSheet.Cell(row, cols.退勤);
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

    private static string? GetString(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty()) return null;
        return cell.GetString().Trim();
    }
}
