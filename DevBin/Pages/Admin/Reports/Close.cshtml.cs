#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Authorization;

namespace DevBin.Pages.Admin.Reports
{
    [Authorize(Roles = "Administrator")]
    public class CloseModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CloseModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Report = await _context.Reports.FindAsync(id);

            if (Report != null)
            {
                Report.IsClosed = true;
                _context.Reports.Update(Report);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
