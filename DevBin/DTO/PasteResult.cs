using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.DTO
{
    public class PasteResult
    {
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? SyntaxId { get; set; }
        public int ExposureId { get; set; }
        public int Views { get; set; }
        public string? Author { get; set; }

    }
}
