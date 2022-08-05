using DevBin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public SearchModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public string Query { get; set; }
        public IList<Paste> Result { get; set; }

        public async Task<IActionResult> OnGetAsync([FromQuery] string query)
        {
            Query = query;
            if(string.IsNullOrEmpty(Query))
            {
                Result = new List<Paste>();
                return Page();
            }

            var search = _context.Pastes.AsQueryable();
            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);
                search = search.Where(q => q.Exposure.IsListed || q.AuthorId == user.Id);
            }
            else
            {
                search = search.Where(q => q.Exposure.IsListed);
            }

            search = search.Where(q => q.Title.ToLower().Contains(Query.ToLower()));
            Result = await search.ToListAsync();
            search.OrderByDescending(q => q.DateTime).Take(_configuration.GetValue<int>("LatestPageSize"));

            return Page();
        }
    }
}
