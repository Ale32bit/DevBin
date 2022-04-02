using DevBin.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using DevBin.Data;
using DevBin.Utils;
using Microsoft.AspNetCore.Identity;

namespace DevBin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }
            [DataType(DataType.Text)]
            public string? Title { get; set; }
            [Required]
            public int SyntaxId { get; set; }
            [Required]
            public int ExposureId { get; set; }
            public bool AsGuest { get; set; }
            public int? FolderId { get; set; }

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

        public async Task OnPostAsync()
        {
            Input.AsGuest = !User.Identity.IsAuthenticated || Input.AsGuest;

            var paste = new Paste {
                Title = Input.Title ?? "Unnamed Paste",
                Cache = PasteUtils.GetShortContent(Input.Content, 128),
                Content = Input.Content,
                ExposureId = Input.ExposureId,
                SyntaxId = Input.SyntaxId,
                
            };

            if (!Input.AsGuest)
                paste.AuthorId = _userManager.GetUserId(User);

            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();
        }
    }
}