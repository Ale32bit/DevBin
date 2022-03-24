#nullable disable
using DevBin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages.Admin.Pastes
{
    public class IndexModel : PageModel
    {
        private readonly DevBin.Data.ApplicationDbContext _context;

        public IndexModel(DevBin.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Paste> Paste { get; set; }

        public async Task OnGetAsync()
        {
            Paste = await _context.Pastes
                .Include(p => p.Author)
                .Include(p => p.Folder)
                .Include(p => p.Syntax).ToListAsync();
        }
    }
}
