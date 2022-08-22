using System.ComponentModel.DataAnnotations;

namespace DevBin.Models;

#nullable disable
public class LockedIpAddresses
{
    public int Id { get; set; }
    [MaxLength(45)]
    public string IpAddress { get; set; }
    public DateTime LockDatetime { get; set; }
    public DateTime UnlockDateTime { get; set; }
    public string? Reason { get; set; }

}
