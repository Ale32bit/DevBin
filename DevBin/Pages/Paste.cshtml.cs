using System;
using System.Linq;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace DevBin.Pages
{
    public class PasteModel : PageModel
    {
        private readonly Context _context;
        private readonly PasteStore _pasteStore;
        private readonly IMemoryCache _cache;

        public PasteModel(Context context, PasteStore pasteStore, IMemoryCache cache)
        {
            _context = context;
            _pasteStore = pasteStore;
            _cache = cache;
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

            Paste.Content = _pasteStore.Read(Paste.Code);

            if (!_cache.TryGetValue($"SEEN:{Paste.Code}.{HttpContext.Items["SessionId"]}", out _))
            {
                Paste.Views++;
                _cache.Set($"SEEN:{Paste.Code}.{HttpContext.Items["SessionId"]}", true, TimeSpan.FromHours(2));
                _context.Update(Paste);
                await _context.SaveChangesAsync();
            }

            return Page();
        }
    }
}
