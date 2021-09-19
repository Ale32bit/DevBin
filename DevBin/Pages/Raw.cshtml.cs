using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class RawModel : PageModel
    {
        private readonly Context _context;
        private readonly PasteStore _pasteStore;
        private readonly IMemoryCache _cache;

        public RawModel(Context context, PasteStore pasteStore, IMemoryCache cache)
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

            if (Paste.Exposure.IsPrivate)
            {
                if (!HttpContext.User.Identity!.IsAuthenticated)
                {
                    return Unauthorized();
                }

                var currentUser = await _context.Users.FirstOrDefaultAsync(q => q.Email == HttpContext.User.Identity.Name);
                if (currentUser == null || currentUser.Id != Paste.AuthorId)
                {
                    return Forbid();
                }
            }

            if (!_cache.TryGetValue("PASTE:" + Paste.Code, out string content))
            {
                content = await _cache.GetOrCreateAsync("PASTE:" + Paste.Code, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return Task.FromResult(_pasteStore.Read(Paste.Code));
                });
            }

            return Content(content);
        }
    }
}
