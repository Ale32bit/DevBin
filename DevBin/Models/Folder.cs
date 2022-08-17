using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
#nullable disable
namespace DevBin.Models;
public class Folder
{
    public int Id { get; set; }
    [MaxLength(255)]
    public string Name { get; set; }
    [MaxLength(255)]
    public string Link { get; set; }
    public int OwnerId { get; set; }
    public DateTime DateTime { get; set; }

    public virtual ApplicationUser Owner { get; set; }
    public virtual IEnumerable<Paste> Pastes { get; set; }

    public int GetPublicPastesCount()
    {
        return Pastes.Where(q => q.Exposure.IsListed).Count();
    }

    public int GetPastesCount()
    {
        return Pastes.Count();
    }

    public static string GenerateLink(string name)
    {
        name = name.Replace(' ', '-');
        return Regex.Replace(name.ToLower(), @"[^a-z0-9_]+", "-");
    }
}
