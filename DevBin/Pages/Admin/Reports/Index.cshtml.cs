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
    public class IndexModel : PageModel
    {
        private readonly DevBin.Data.ApplicationDbContext _context;

        public IndexModel(DevBin.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Report> Report { get;set; }

        public async Task OnGetAsync()
        {
            Report = await _context.Reports
                .Include(r => r.Paste)
                .Include(r => r.Reporter).ToListAsync();
        }
    }
}
