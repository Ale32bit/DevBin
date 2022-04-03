using System.ComponentModel.DataAnnotations;

namespace DevBin.Models;

public class Exposure
{
    public int Id { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    public bool IsListed { get; set; }
    public bool IsAuthorOnly { get; set; }
}