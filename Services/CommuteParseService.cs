using ClosedXML.Excel;
using KinmuReport.Models;
using Microsoft.Extensions.Options;

namespace KinmuReport.Services;

public class CommuteParseService
{
    private readonly CommuteParseSettings _settings;

    public CommuteParseService(IOptions<CommuteParseSettings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// 通勤手当Excelをパースする
    /// </summary>
    /// <param name="stream">Excelファイルのストリーム</param>
    /// <param name="fallback社員番号">Excel内に社員番号がない場合に使用する社員番号</param>
    /// <returns>社員番号、対象年月、通勤手当リスト</returns>
    public (string 社員番号, int 対象年月, List<通勤手当> 通勤手当リスト) ParseExcel(Stream stream, string? fallback社員番号 = null)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheet(_settings.SheetName);

        // ヘッダー情報を取得
        var 年 = sheet.Cell(_settings.Header.年).GetValue<int>();
        var 月 = sheet.Cell(_settings.Header.月).GetValue<int>();
        var 対象年月 = 年 * 100 + 月;

        // 社員番号（空欄ならfallbackを使用）
        var 社員番号Cell = sheet.Cell(_settings.Header.社員番号);
        var 社員番号 = 社員番号Cell.IsEmpty() ? null : 社員番号Cell.GetString().Trim();
        if (string.IsNullOrEmpty(社員番号))
        {
            社員番号 = fallback社員番号 ?? throw new InvalidOperationException("社員番号が指定されていません");
        }

        // 日別データを取得（左右両方）
        var 通勤手当リスト = new List<通勤手当>();

        for (int row = _settings.DataStartRow; row <= _settings.DataEndRow; row++)
        {
            // 左半分のデータ
            var leftRecord = ParseRow(sheet, row, _settings.DataLeft, 社員番号, 対象年月);
            if (leftRecord != null)
            {
                通勤手当リスト.Add(leftRecord);
            }

            // 右半分のデータ
            var rightRecord = ParseRow(sheet, row, _settings.DataRight, 社員番号, 対象年月);
            if (rightRecord != null)
            {
                通勤手当リスト.Add(rightRecord);
            }
        }

        return (社員番号, 対象年月, 通勤手当リスト);
    }

    private static 通勤手当? ParseRow(IXLWorksheet sheet, int row, CommuteParseSettings.DataColumns cols, string 社員番号, int 対象年月)
    {
        var 日付Cell = sheet.Cell(row, cols.日付);
        if (日付Cell.IsEmpty()) return null;

        // 日付がDateTime形式で入っている
        if (!日付Cell.Value.IsDateTime) return null;

        var 日付 = DateOnly.FromDateTime(日付Cell.GetDateTime());

        // 金額または経路NOがある行のみ取り込む
        var 金額 = GetDecimal(sheet, row, cols.金額);
        var 経路NO = GetInt(sheet, row, cols.経路NO);

        // 金額も経路NOも空なら、通勤なし（在宅等）としてスキップ
        if (!金額.HasValue && !経路NO.HasValue) return null;

        return new 通勤手当
        {
            社員番号 = 社員番号,
            対象年月 = 対象年月,
            日付 = 日付,
            経路NO = 経路NO,
            金額 = 金額,
            備考 = GetString(sheet, row, cols.備考)
        };
    }

    private static decimal? GetDecimal(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty() || !cell.Value.IsNumber) return null;
        return cell.GetValue<decimal>();
    }

    private static int? GetInt(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty() || !cell.Value.IsNumber) return null;
        return cell.GetValue<int>();
    }

    private static string? GetString(IXLWorksheet sheet, int row, int col)
    {
        var cell = sheet.Cell(row, col);
        if (cell.IsEmpty()) return null;
        return cell.GetString().Trim();
    }
}
