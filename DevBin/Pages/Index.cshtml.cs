using DevBin.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using DevBin.Data;

namespace DevBin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }
            [Required]
            [DataType(DataType.Text)]
            public string Title { get; set; }
            [Required]
            public int SyntaxId { get; set; }
            [Required]
            public Exposure Exposure { get; set; }
            public bool AsGuest { get; set; }

        }

        [BindProperty]
        public IList<Paste> Latest { get; set; } = new List<Paste>();

        public async Task OnGetAsync()
        {
            var exposures = _context.Exposures.AsQueryable();
            if (!User.Identity.IsAuthenticated)
                exposures = exposures.Where(q => !q.RequireLogin);

            ViewData["Exposures"] = new SelectList(exposures, "Id", "Name", 1);

            ViewData["Syntaxes"] = new SelectList(_context.Syntaxes.Where(q => !q.IsHidden), "Id", "DisplayName", 1);
        }
    }
}