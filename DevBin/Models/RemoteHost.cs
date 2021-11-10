using System;
using System.Collections.Generic;

namespace DevBin.Models
{
    /// <summary>
    /// List of cached IP addresses for security purposes.
    /// Both IPv4 and IPv6
    /// </summary>
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
