using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DevBin.Data;
using DevBin.Models;

namespace DevBin.Pages
{
    public class EditModel : PageModel
    {
        private readonly DevBin.Data.Context _context;

        public EditModel(DevBin.Data.Context context)
        {
            _context = context;
        }

        [BindProperty]
        public Paste Paste { get; set; }

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

            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Username");
            ViewData["ExposureId"] = new SelectList(_context.Exposures, "Id", "Name");
            ViewData["SyntaxId"] = new SelectList(_context.Syntaxes, "Id", "Pretty");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Paste).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PasteExists(Paste.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PasteExists(int id)
        {
            return _context.Pastes.Any(e => e.Id == id);
        }
    }
}
