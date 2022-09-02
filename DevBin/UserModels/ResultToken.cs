namespace DevBin.UserModels;
#nullable disable
public class ResultToken
{
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool AllowGet { get; set; }
    public bool AllowCreate { get; set; }
    public bool AllowUpdate { get; set; }
    public bool AllowDelete { get; set; }
    public bool AllowGetUser { get; set; }
    public bool AllowCreateFolders { get; set; }
    public bool AllowDeleteFolders { get; set; }

    public static ResultToken From(ApiToken token)
    {
        return new ResultToken
        {
            Name = token.Name,
            CreatedAt = token.CreatedAt,
            AllowGet = token.AllowGet,
            AllowCreate = token.AllowCreate,
            AllowUpdate = token.AllowUpdate,
            AllowDelete = token.AllowDelete,
            AllowGetUser = token.AllowGetUser,
            AllowCreateFolders = token.AllowCreateFolders,
            AllowDeleteFolders = token.AllowDeleteFolders,
        };
    }
}
