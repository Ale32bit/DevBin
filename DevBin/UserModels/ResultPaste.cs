namespace DevBin.UserModels;
#nullable disable
public class ResultPaste
{
    public string Code { get; set; }
    public string Title { get; set; }
    public int Views { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string SyntaxName { get; set; }
    public int ExposureId { get; set; }
    public string? Author { get; set; }
    public int? FolderId { get; set; }

    public static ResultPaste From(Paste paste)
    {
        return new ResultPaste
        {
            Code = paste.Code,
            Title = paste.Title,
            Views = paste.Views,
            CreatedAt = paste.DateTime,
            UpdatedAt = paste.UpdateDatetime,
            SyntaxName = paste.SyntaxName,
            ExposureId = paste.ExposureId,
            Author = paste.Author?.UserName,
            FolderId = paste.FolderId,
        };
    }
}
