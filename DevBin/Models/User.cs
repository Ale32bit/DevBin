using System;
using System.Collections.Generic;

#nullable disable

namespace DevBin.Models
{
    public partial class User
    {
        public User()
        {
            Pastes = new HashSet<Paste>();
            Sessions = new HashSet<Session>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public byte[] Password { get; set; }
        public string ApiToken { get; set; }
        public bool Verified { get; set; }
        public string ActionCode { get; set; }
        public DateTime ActionDate { get; set; }

        public virtual ICollection<Paste> Pastes { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
    }
}
