using Microsoft.AspNetCore.Identity;
using System.Net;

namespace DevBin.Models;
public class Paste
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; } = "Unnamed paste";
    public int Views { get; set; }
    public DateTime Datetime { get; set; }
    public DateTime? UpdateDatetime { get; set; }
    public string Cache { get; set; }
    public string Content { get; set; }
    public IPAddress UploaderIPAddress { get; set; }
    public int SyntaxId { get; set; } = 1;
    public int ExposureId { get; set; } = 1;
    public string AuthorId { get; set; }
    public int? FolderId { get; set; }

    public virtual Syntax Syntax { get; set; }
    public virtual Exposure Exposure { get; set; }
    public virtual ApplicationUser Author { get; set; }
    public virtual Folder? Folder { get; set; }
    public virtual IList<Report> Reports { get; set; }

}
