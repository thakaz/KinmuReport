using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[PrimaryKey("社員番号", "対象年月", "日付")]
[Table("通勤手当")]
public partial class 通勤手当
{
    [Key]
    [StringLength(20)]
    public string 社員番号 { get; set; } = null!;

    [Key]
    public int 対象年月 { get; set; }

    [Key]
    public DateOnly 日付 { get; set; }

    public int? 経路NO { get; set; }

    [Precision(10, 0)]
    public decimal? 金額 { get; set; }

    [StringLength(500)]
    public string? 備考 { get; set; }

    [ForeignKey("社員番号")]
    [InverseProperty("通勤手当s")]
    public virtual 社員 社員番号Navigation { get; set; } = null!;
}
