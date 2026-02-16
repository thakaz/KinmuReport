using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[Table("社員")]
[Index("ログインid", Name = "社員_ログインid_key", IsUnique = true)]
public partial class 社員
{
    [Key]
    [StringLength(20)]
    public string 社員番号 { get; set; } = null!;

    [StringLength(100)]
    public string 社員名 { get; set; } = null!;

    [StringLength(50)]
    public string ログインid { get; set; } = null!;

    [StringLength(256)]
    public string? パスワードハッシュ { get; set; }

    [StringLength(256)]
    public string? adオブジェクトid { get; set; }

    [StringLength(20)]
    public string? グループコード { get; set; }

    [StringLength(20)]
    public string 権限 { get; set; } = null!;

    [InverseProperty("社員番号Navigation")]
    public virtual ICollection<アップロード履歴> アップロード履歴s { get; set; } = new List<アップロード履歴>();

    [ForeignKey("グループコード")]
    [InverseProperty("社員s")]
    public virtual グループ? グループコードNavigation { get; set; }

    [InverseProperty("ロック者番号Navigation")]
    public virtual ICollection<ロック> ロックロック者番号Navigations { get; set; } = new List<ロック>();

    [InverseProperty("社員番号Navigation")]
    public virtual ICollection<ロック> ロック社員番号Navigations { get; set; } = new List<ロック>();

    [InverseProperty("社員番号Navigation")]
    public virtual ICollection<勤怠> 勤怠s { get; set; } = new List<勤怠>();

    [InverseProperty("社員番号Navigation")]
    public virtual ICollection<通勤手当> 通勤手当s { get; set; } = new List<通勤手当>();
}
