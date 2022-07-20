namespace DevBin.UserModels;
public class UserPaste
{
    public string? Title { get; set; }
    public string SyntaxName { get; set; }
    public int ExposureId { get; set; }
    public string Content { get; set; }
    public int? FolderId { get; set; }
    public bool? AsGuest { get; set; }
}
