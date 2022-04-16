using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public DbSet<Paste> Pastes { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Syntax> Syntaxes { get; set; }
        public DbSet<Exposure> Exposures { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ApiToken> ApiTokens { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}