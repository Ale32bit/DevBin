using System;

namespace DevBin.DTO
{
    public class PasteResult
    {
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string SyntaxId { get; set; }
        public int ExposureId { get; set; }
        public int Views { get; set; }
        public string? Author { get; set; }

    }
}
