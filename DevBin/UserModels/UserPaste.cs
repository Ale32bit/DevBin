namespace DevBin.UserModels;

public class UserPaste
{
    public string? Code { get; set; }
    public string? Title { get; set; }
    public int? Views { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int SyntaxId { get; set; }
    public int ExposureId { get; set; }
    public string? Author { get; set; }
    public int? FolderId { get; set; }
    public string? Content { get; set; }
    public bool? AsGuest { get; set; }

    public static UserPaste From(Paste paste)
    {
        return new UserPaste
        {
            Code = paste.Code,
            Title = paste.Title,
            Views = paste.Views,
            CreatedAt = paste.DateTime,
            UpdatedAt = paste.UpdateDatetime,
            SyntaxId = paste.SyntaxId,
            ExposureId = paste.ExposureId,
            Author = paste.Author?.UserName,
            FolderId = paste.FolderId,
        };
    }
}
