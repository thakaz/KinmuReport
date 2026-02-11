using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[PrimaryKey("社員番号", "対象年月")]
[Table("アップロード履歴")]
public partial class アップロード履歴
{
    [Key]
    [StringLength(20)]
    public string 社員番号 { get; set; } = null!;

    [Key]
    [StringLength(7)]
    public string 対象年月 { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime アップロード日時 { get; set; }

    public int? sp版数 { get; set; }

    [ForeignKey("社員番号")]
    [InverseProperty("アップロード履歴s")]
    public virtual 社員 社員番号Navigation { get; set; } = null!;
}
