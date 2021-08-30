using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class UserModel : PageModel
    {
        private readonly Context _context;

        public UserModel(Context context)
        {
            _context = context;
        }

        public User PasteUser { get; set; }
        public IList<Paste> Pastes { get; set; }

        public async Task<IActionResult> OnGetAsync(string username)
        {
            PasteUser = await _context.Users.Where(q => q.Username == username).FirstOrDefaultAsync();

            if (PasteUser == null)
            {
                return NotFound();
            }

            var pastes = _context.Pastes.Where(q => q.AuthorId == PasteUser.Id);

            bool isAuthor = false;
            if (HttpContext.User.Identity is { IsAuthenticated: true })
            {
                isAuthor = HttpContext.User.Identity.Name == PasteUser.Email;
            }

            if (!isAuthor)
            {
                pastes = pastes.Where(q => !q.Exposure.IsPrivate);
            }

            Pastes = await pastes.OrderByDescending(q => q.Datetime).ToListAsync();

            return Page();
        }
    }
}
