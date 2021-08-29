using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace DevBin.Models
{
    public partial class Paste
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int? SyntaxId { get; set; }
        public int ExposureId { get; set; }
        public int? AuthorId { get; set; }
        public int Views { get; set; }
        public DateTime Datetime { get; set; }
        public DateTime? UpdateDatetime { get; set; }
        public string Cache { get; set; }

        public virtual User Author { get; set; }
        public virtual Exposure Exposure { get; set; }
        public virtual Syntaxes Syntax { get; set; }

        [NotMapped]
        public string Content { get; set; }
    }
}
