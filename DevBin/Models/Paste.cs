using Microsoft.AspNetCore.Identity;

namespace DevBin.Models
{
    public class Paste
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; } = "Unnamed paste";
        public int Views { get; set; }
        public DateTime Datetime { get; set; }
        public DateTime? UpdateDatetime { get; set; }
        public string Cache { get; set; }
        public string Content { get; set; }
        public int SyntaxId { get; set; } = 1;
        public int ExposureId { get; set; } = 0;
        public string AuthorId { get; set; }
        public int? FolderId { get; set; }

        public virtual Syntax Syntax { get; set; }
        public virtual Exposure Exposure { get; set; } = Exposure.Public;
        public virtual IdentityUser Author { get; set; }
        public virtual Folder? Folder { get; set; }

    }
}
