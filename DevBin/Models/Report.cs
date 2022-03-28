using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace DevBin.Models;
public class Report
{
    public int Id { get; set; }
    [MaxLength(8)]
    public int PasteId { get; set; }
    public string Reason { get; set; }
    public IPAddress ReporterIPAddress { get; set; }
    public string? ReporterId { get; set; }

    public virtual Paste Paste { get; set; }
    public virtual ApplicationUser? Reporter { get; set; }
}
