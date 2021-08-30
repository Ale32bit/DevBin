using System;

#nullable disable

namespace DevBin.Models
{
    public partial class Session
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public DateTime Datetime { get; set; }

        public virtual User User { get; set; }
    }
}
