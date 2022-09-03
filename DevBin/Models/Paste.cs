using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
#nullable disable
namespace DevBin.Models;
public class Paste
{
    public int Id { get; set; }
    [MaxLength(8)]
    public string Code { get; set; }
    [MaxLength(255)]
    public string Title { get; set; } = "Unnamed paste";
    public int Views { get; set; }
    public DateTime DateTime { get; set; }
    public DateTime? UpdateDatetime { get; set; }
    [MaxLength(255)]
    public string Cache { get; set; }
    public byte[] Content { get; set; }
    [MaxLength(45)]
    public string UploaderIPAddress { get; set; }
    public string SyntaxName { get; set; } = "plaintext";
    public DateTime? ExpireDate { get; set; }
    public int ExposureId { get; set; } = 1;
    public int? AuthorId { get; set; }
    public int? FolderId { get; set; }

    public virtual Syntax Syntax { get; set; }
    public virtual Exposure Exposure { get; set; }
    public virtual ApplicationUser? Author { get; set; }
    public virtual Folder? Folder { get; set; }
    public virtual ICollection<Report> Reports { get; set; }

    [NotMapped]
    internal virtual string StringContent
    {
        get => Encoding.UTF8.GetString(Content);
    }
}
