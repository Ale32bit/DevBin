using System.ComponentModel.DataAnnotations;
#nullable disable
namespace DevBin.Models;
public class Syntax
{
    [Key]
    [MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string DisplayName { get; set; }

    public bool IsHidden { get; set; }
}
