using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages
{
    public class PasteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public PasteModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public Paste Paste { get; set; }
        public bool IsAuthor { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(string? code)
        {
            if (code == null)
                return NotFound();

            Paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);

            if (Paste == null)
                return NotFound();

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (Paste.Exposure.IsAuthorOnly)
            {
                if (loggedInUser == null || loggedInUser.Id != Paste.Author.Id)
                    return NotFound();
            }

            ViewData["Title"] = Paste.Title;

            IsAuthor = Paste.Author != null && Paste.Author.Id == loggedInUser.Id;

            return Page();
        }
    }
}
