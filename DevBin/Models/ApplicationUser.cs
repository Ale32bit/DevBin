using Microsoft.AspNetCore.Identity;

namespace DevBin.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual IList<Paste> Pastes { get; set; }
        public virtual IList<Folder> Folders { get; set; }

    }
}
