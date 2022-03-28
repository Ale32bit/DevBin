using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace DevBin.Models;
public class Folder
{
    public int Id { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    public string OwnerId { get; set; }

    public virtual ApplicationUser Owner { get; set; }
    public virtual IList<Paste> Pastes { get; set; }
}
