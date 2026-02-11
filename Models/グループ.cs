using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinmuReport.Models;

[Table("グループ")]
public partial class グループ
{
    [Key]
    [StringLength(20)]
    public string グループコード { get; set; } = null!;

    [StringLength(100)]
    public string グループ名 { get; set; } = null!;

    [InverseProperty("グループコードNavigation")]
    public virtual ICollection<社員> 社員s { get; set; } = new List<社員>();
}
