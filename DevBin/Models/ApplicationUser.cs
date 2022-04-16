using Microsoft.AspNetCore.Identity;

namespace DevBin.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string? LegacyPassword { get; set; }

        public virtual ICollection<Paste> Pastes { get; set; }
        public virtual ICollection<Folder> Folders { get; set; }
        public virtual ICollection<ApiToken> ApiTokens { get; set; }
    }
}