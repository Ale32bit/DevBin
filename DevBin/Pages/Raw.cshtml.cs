﻿using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class RawModel : PageModel
    {
        private readonly Context _context;
        private readonly PasteStore _pasteStore;

        public RawModel(Context context, PasteStore pasteStore)
        {
            _context = context;
            _pasteStore = pasteStore;
        }

        public Paste Paste { get; set; }

#nullable enable
        public async Task<IActionResult> OnGetAsync(string? code)
        {
            if (code == null)
            {
                return NotFound();
            }

            Paste = await _context.Pastes
                .Include(p => p.Author)
                .Include(p => p.Exposure)
                .Include(p => p.Syntax)
                .FirstOrDefaultAsync(m => m.Code == code);

            if (Paste == null)
            {
                return NotFound();
            }



            return Content(_pasteStore.Read(code));
        }
    }
}
