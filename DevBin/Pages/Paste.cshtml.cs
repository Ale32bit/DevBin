using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class PasteModel : PageModel
    {
        private readonly Context _context;
        private readonly IMemoryCache _cache;

        public PasteModel(Context context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public Paste Paste { get; set; }
        public string Size { get; set; }
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
                    return Unauthorized();
                }
            }

            if (_cache.TryGetValue("PASTE:" + Paste.Code, out string pasteContent))
            {
                Paste.Content = pasteContent;
            }
            else
            {
                Paste.Content = await _cache.GetOrCreateAsync("PASTE:" + Paste.Code, entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return Task.FromResult(Paste.Content);
                });
            }

            Size = Utils.FriendlySize(Paste.Content.Length);


            if (!_cache.TryGetValue($"SEEN:{Paste.Code}.{HttpContext.Items["SessionId"]}", out _))
            {
                Paste.Views++;
                _cache.Set($"SEEN:{Paste.Code}.{HttpContext.Items["SessionId"]}", true, TimeSpan.FromHours(2));
                _context.Update(Paste);
                await _context.SaveChangesAsync();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDeleteAsync(string? code)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(q => q.Email == HttpContext.User.Identity.Name);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
                return NotFound();

            if (paste.AuthorId != currentUser.Id)
            {
                return Unauthorized();
            }

            _context.Pastes.Remove(paste);
            await _context.SaveChangesAsync();

            _cache.Remove("PASTE:" + paste.Code);

            return Redirect("/");
        }
    }
}
