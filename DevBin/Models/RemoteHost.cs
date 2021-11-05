using System;
using System.Collections.Generic;

#nullable disable

namespace DevBin.Models
{
    public partial class RemoteHost
    {
        public int Id { get; set; }
        public byte[] Address { get; set; }
        public DateTime LastCheckDate { get; set; }
        public DateTime ExpireDate { get; set; }
        public string Note { get; set; }
        public bool Blocked { get; set; }
    }
}
