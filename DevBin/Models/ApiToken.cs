namespace DevBin.Models;

public class ApiToken
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public string OwnerId { get; set; }
    public virtual ApplicationUser Owner { get; set; }

    public bool AllowGet { get; set; }
    public bool AllowCreate { get; set; }
    public bool AllowUpdate { get; set; }
    public bool AllowDelete { get; set; }
    public bool AllowGetUser { get; set; }
    public bool AllowCreateFolders { get; set; }
    public bool AllowDeleteFolders { get; set; }
}