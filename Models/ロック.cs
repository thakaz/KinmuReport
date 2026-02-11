using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[PrimaryKey("社員番号", "対象年月")]
[Table("ロック")]
public partial class ロック
{
    [Key]
    [StringLength(20)]
    public string 社員番号 { get; set; } = null!;

    [Key]
    public int 対象年月 { get; set; }

    [StringLength(20)]
    public string ロック者番号 { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime ロック日時 { get; set; }

    [ForeignKey("ロック者番号")]
    [InverseProperty("ロックロック者番号Navigations")]
    public virtual 社員 ロック者番号Navigation { get; set; } = null!;

    [ForeignKey("社員番号")]
    [InverseProperty("ロック社員番号Navigations")]
    public virtual 社員 社員番号Navigation { get; set; } = null!;
}
