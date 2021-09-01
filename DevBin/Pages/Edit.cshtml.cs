using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class EditModel : PageModel
    {
        private readonly Context _context;
        private readonly PasteStore _pasteStore;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public EditModel(Context context, PasteStore pasteStore, IMemoryCache cache, IConfiguration configuration)
        {
            _context = context;
            _pasteStore = pasteStore;
            _cache = cache;
            _configuration = configuration;
        }

        [BindProperty]
        public UserPasteForm UserPaste { get; set; }
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
                .Include(p => p.Syntax).FirstOrDefaultAsync(m => m.Code == code);

            if (Paste == null)
            {
                return NotFound();
            }

            if (HttpContext.User.Identity is { IsAuthenticated: true } && Paste.Exposure.AllowEdit)
            {
                var currentUser = _context.Users.FirstOrDefault(q => q.Email == Paste.Author.Email);
                if (currentUser == null || Paste.AuthorId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else
            {
                return Unauthorized();
            }

            ViewData["ExposureId"] = new SelectList(_context.Exposures, "Id", "Name");
            ViewData["SyntaxId"] = new SelectList(_context.Syntaxes, "Id", "Pretty");

            UserPaste = new()
            {
                Content = _pasteStore.Read(Paste.Code),
                AsGuest = false,
                Title = Paste.Title,
                ExposureId = Paste.ExposureId,
                SyntaxId = Paste.SyntaxId,
            };

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(string? code)
        {
            if (UserPaste.Content.Length > _configuration.GetValue<long>("PasteMaxSize"))
            {
                ModelState.AddModelError("UserPaste.Content", "The content is too big!");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Paste = _context.Pastes.FirstOrDefault(q => q.Code == code);
            if (Paste == null)
            {
                return NotFound();
            }

            if (HttpContext.User.Identity is { IsAuthenticated: true } && Paste.Exposure.AllowEdit)
            {
                var currentUser = _context.Users.FirstOrDefault(q => q.Email == Paste.Author.Email);
                if (currentUser == null || Paste.AuthorId != currentUser.Id)
                {
                    return Forbid();
                }
            }
            else
            {
                return Unauthorized();
            }

            Paste.ExposureId = UserPaste.ExposureId;
            Paste.SyntaxId = UserPaste.SyntaxId;
            Paste.Title = UserPaste.Title;
            Paste.Cache = UserPaste.Content[0..Math.Min(UserPaste.Content.Length, 255)];

            Paste.UpdateDatetime = DateTime.UtcNow;

            _context.Attach(Paste).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _pasteStore.Write(Paste.Code, UserPaste.Content);
                _cache.Remove("PASTE:" + Paste.Code);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PasteExists(Paste.Code))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Redirect("/" + Paste.Code);
        }

        private bool PasteExists(string code)
        {
            return _context.Pastes.Any(e => e.Code == code);
        }
    }
}
