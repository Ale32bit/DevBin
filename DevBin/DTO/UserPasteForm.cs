namespace DevBin.Models
{
    public class UserPasteForm
    {
        public string Title { get; set; }
        public int? SyntaxId { get; set; }
        public int ExposureId { get; set; }
        public string Content { get; set; }
        public bool AsGuest { get; set; } = false;
    }
}