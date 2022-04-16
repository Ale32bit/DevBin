using System.ComponentModel.DataAnnotations;

namespace DevBin.Models;
public class Report
{
    public int Id { get; set; }
    public int PasteId { get; set; }
    public string Reason { get; set; }
    [MaxLength(42)]
    public string ReporterIPAddress { get; set; }
    public int? ReporterId { get; set; }
    public bool IsClosed { get; set; }

    public virtual Paste Paste { get; set; }
    public virtual ApplicationUser? Reporter { get; set; }
}
