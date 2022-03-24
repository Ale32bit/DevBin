#nullable disable
using DevBin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages.Admin.Reports
{
    public class IndexModel : PageModel
    {
        private readonly DevBin.Data.ApplicationDbContext _context;

        public IndexModel(DevBin.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Report> Report { get; set; }

        public async Task OnGetAsync()
        {
            Report = await _context.Reports
                .Include(r => r.Paste)
                .Include(r => r.Reporter).ToListAsync();
        }
    }
}
