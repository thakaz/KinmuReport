using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KinmuReport.Models;

[Table("監査ログ")]
public class 監査ログ
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime 操作日時 {  get; set;} = DateTime.Now;

    [Required]
    public string 操作種別 { get; set; } = "";//INSERT,UPDATE,DELETE

    [Required]
    public string テーブル名 { get; set; } = "";

    public string? レコードキー { get; set; } //対象レコードの主キー

    public string? 操作者{ get; set; }//社員番号

    public string? 変更内容 { get; set; } //変更前と変更後の内容をJSONで保存。


}
