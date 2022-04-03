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
using Microsoft.AspNetCore.Identity;

namespace DevBin.Pages.User
{
    public class FolderModel : PageModel
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public FolderModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        public Folder Folder { get; set; }
        public IEnumerable<Paste> Pastes { get; set; }


        public async Task<IActionResult> OnGetAsync(string? username, int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Folder = await _context.Folders
                .Include(f => f.Owner).FirstOrDefaultAsync(m => m.Id == id);

            if (Folder == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound($"User {username} does not exist!");

            ViewData["Title"] = $"Folder {Folder.Name}";
            ViewData["Username"] = user.UserName;

            Pastes = _context.Pastes.Where(q => q.AuthorId == user.Id && q.FolderId == Folder.Id).OrderByDescending(q => q.DateTime);

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (!_signInManager.IsSignedIn(User) || user.Id != loggedInUser.Id)
            {
                Pastes = Pastes.Where(q => q.Exposure.IsListed);
            }

            return Page();
        }
    }
}
