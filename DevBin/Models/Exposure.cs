using System.Collections.Generic;

#nullable disable

namespace DevBin.Models
{
    public partial class Exposure
    {
        public Exposure()
        {
            Pastes = new HashSet<Paste>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public bool RegisteredOnly { get; set; }
        public bool AllowEdit { get; set; }
        public bool IsPrivate { get; set; }

        public virtual ICollection<Paste> Pastes { get; set; }
    }
}
