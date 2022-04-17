using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages
{
    public class LatestModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public LatestModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public IList<Paste> Pastes { get; set; }

        public async Task OnGetAsync()
        {
            Pastes = await _context.Pastes
                .Where(q => q.Exposure.IsListed)
                .OrderByDescending(q => q.DateTime)
                .Take(_configuration.GetValue<int>("LatestPageSize"))
                .ToListAsync();
        }
    }
}
