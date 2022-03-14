using DevBin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Paste> Pastes { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Syntax> Syntaxes { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}