using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class LatestModel : PageModel
    {
        private readonly Context _context;

        public LatestModel(Context context)
        {
            _context = context;
        }

        public IList<Paste> Paste { get; set; }

        public async Task OnGetAsync()
        {
            Paste = await _context.Pastes
                .Where(q => q.ExposureId == 1) // 1 = Public
                .Include(p => p.Exposure)
                .Include(p => p.Syntax)
                .ToListAsync();
            Paste = Paste.Reverse().Take(30).ToList();

        }
    }
}