#nullable disable
namespace DevBin.UserModels;
public class ResultFolder
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ResultFolder From(Folder folder)
    {
        return new ResultFolder
        {
            Id = folder.Id,
            Name = folder.Name,
            CreatedAt = folder.DateTime,
        };
    }
}
