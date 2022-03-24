#nullable disable
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages.Admin.Reports
{
    public class DetailsModel : PageModel
    {
        private readonly DevBin.Data.ApplicationDbContext _context;

        public DetailsModel(DevBin.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Report Report { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Report = await _context.Reports
                .Include(r => r.Paste)
                .Include(r => r.Reporter).FirstOrDefaultAsync(m => m.Id == id);

            if (Report == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
