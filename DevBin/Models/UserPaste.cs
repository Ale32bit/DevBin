namespace DevBin.Models
{
    public class UserPaste
    {
        public string Title { get; set; }
        public int? SyntaxId { get; set; }
        public int ExposureId { get; set; }
        public virtual Exposure Exposure { get; set; }
        public virtual Syntaxes Syntax { get; set; }
        public string Content { get; set; }
        public bool AsGuest { get; set; }
    }
}