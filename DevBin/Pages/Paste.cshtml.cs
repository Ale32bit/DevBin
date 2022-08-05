using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DevBin.Pages
{
    public class PasteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;

        public PasteModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
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
                if (loggedInUser == null || loggedInUser.Id != Paste.Author!.Id)
                    return NotFound();
            }

            var session = Utils.Utils.GetUserSessionID(HttpContext, Paste.Code);
            var hasViewed = await _cache.GetAsync(session);
            if (hasViewed == null)
            {
                Paste.Views++;
                _context.Update(Paste);
                await _context.SaveChangesAsync();
                await _cache.SetAsync(session, new byte[] { 1 }, new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromHours(2),
                });
            }
            else
            {
                await _cache.RefreshAsync(session);
            }

            ViewData["Title"] = Paste.Title;
            ViewData["MetaDescription"] = Paste.Cache.Replace('\n', ' ');
            IsAuthor = Paste.Author != null && Paste.Author.Id == loggedInUser?.Id;

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string? code)
        {
            if (!_signInManager.IsSignedIn(User))
                return Unauthorized();

            if (code == null)
                return NotFound();

            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
                return NotFound();

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (paste.AuthorId != loggedInUser.Id)
                return Unauthorized();

            _context.Remove(paste);
            await _context.SaveChangesAsync();


            return Redirect("/");
        }

        public async Task<IActionResult> OnGetDownloadAsync(string? code)
        {
            if (code == null)
                return NotFound();

            Paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);

            if (Paste == null)
                return NotFound();

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (Paste.Exposure.IsAuthorOnly)
            {
                if (loggedInUser == null || loggedInUser.Id != Paste.Author!.Id)
                    return NotFound();
            }

            return File(Paste.Content, "application/octet-stream", Paste.Title);
        }
    }
}