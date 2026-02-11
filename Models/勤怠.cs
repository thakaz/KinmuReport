using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[PrimaryKey("社員番号", "勤務日")]
[Table("勤怠")]
public partial class 勤怠
{
    [Key]
    [StringLength(20)]
    public string 社員番号 { get; set; } = null!;

    [Key]
    public DateOnly 勤務日 { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? 出勤日時 { get; set; }

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? 退勤日時 { get; set; }

    [Precision(5, 2)]
    public decimal? 所定内時間 { get; set; }

    [Precision(5, 2)]
    public decimal? 残業時間 { get; set; }

    [Precision(5, 2)]
    public decimal? 深夜残業時間 { get; set; }

    [Precision(5, 2)]
    public decimal? 合計時間 { get; set; }

    [StringLength(50)]
    public string? 勤怠区分 { get; set; }

    [StringLength(500)]
    public string? 備考 { get; set; }

    [ForeignKey("社員番号")]
    [InverseProperty("勤怠s")]
    public virtual 社員 社員番号Navigation { get; set; } = null!;
}
