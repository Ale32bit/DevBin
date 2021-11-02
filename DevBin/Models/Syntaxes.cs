using System;
using System.Collections.Generic;

#nullable disable

namespace DevBin.Models
{
    public partial class Syntaxes
    {
        public Syntaxes()
        {
            Pastes = new HashSet<Paste>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Pretty { get; set; }
        public bool Show { get; set; }

        public virtual ICollection<Paste> Pastes { get; set; }
    }
}
