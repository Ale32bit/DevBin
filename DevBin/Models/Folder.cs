using Microsoft.AspNetCore.Identity;

namespace DevBin.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }

        public virtual IdentityUser Owner { get; set; }
        public virtual IList<Paste> Pastes { get; set; }
    }
}
