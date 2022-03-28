using System.ComponentModel.DataAnnotations;

namespace DevBin.Models;
public class Syntax
{
    public int Id { get; set; }
    [MaxLength(255)]
    public string DisplayName { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    public bool IsHidden { get; set; }
}
