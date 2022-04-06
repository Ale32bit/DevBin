using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DevBin.Pages
{
    public class RawModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IDistributedCache _cache;

        public RawModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _cache = cache;
        }

        public async Task<IActionResult> OnGetAsync(string? code)
        {
            if (code == null)
                return NotFound();

            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);

            if (paste == null)
                return NotFound();

            var loggedInUser = await _userManager.GetUserAsync(User);
            if (paste.Exposure.IsAuthorOnly)
            {
                if (loggedInUser == null || loggedInUser.Id != paste.Author!.Id)
                    return NotFound();
            }
            
            var session = Utils.Utils.GetUserSessionID(HttpContext, paste.Code);
            var hasViewed = await _cache.GetAsync(session);
            if (hasViewed == null)
            {
                paste.Views++;
                _context.Update(paste);
                await _context.SaveChangesAsync();
                await _cache.SetAsync(session, new byte[] { 1 }, new DistributedCacheEntryOptions {
                    SlidingExpiration = TimeSpan.FromHours(2),
                });
            }
            else
            {
                await _cache.RefreshAsync(session);
            }

            return Content(paste.Content);
        }
    }
}