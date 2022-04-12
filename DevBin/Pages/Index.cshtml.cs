using DevBin.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using DevBin.Data;
using DevBin.Services.HCaptcha;
using DevBin.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevBin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly HCaptcha _hCaptcha;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, HCaptcha hCaptcha)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _hCaptcha = hCaptcha;
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
            public string SyntaxId { get; set; }
            [Required]
            public int ExposureId { get; set; }
            public bool AsGuest { get; set; }
            public int? FolderId { get; set; }

            public string? CaptchaToken { get; set; }
            public bool UseCaptcha { get; set; }
        }

        [BindProperty]
        public IList<Paste> Latest { get; set; }

        [BindProperty]
        public int PasteSpace { get; set; }

        [BindProperty]
        public string MemberSpace { get; set; }

        public async Task OnGetAsync()
        {
            Latest = await _context.Pastes.Where(q => q.Exposure.IsListed).OrderByDescending(q => q.DateTime).Take(3).ToListAsync();

            PasteSpace = _signInManager.IsSignedIn(User) ? _configuration.GetValue<int>("Paste:MaxContentSize:Member") : _configuration.GetValue<int>("Paste:MaxContentSize:Guest", 1024 * 2);
            MemberSpace = Utils.Utils.ToIECFormat(_configuration.GetValue<int>("Paste:MaxContentSize:Member"));

            var exposures = _context.Exposures.AsQueryable();
            if (!_signInManager.IsSignedIn(User))
            {
                exposures = exposures.Where(q => !q.IsAuthorOnly);
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                ViewData["Folders"] = new SelectList(_context.Folders.Where(q => q.OwnerId == user.Id), "Id", "Name");
            }

            ViewData["Exposures"] = new SelectList(exposures, "Id", "Name", 1);

            ViewData["Syntaxes"] = new SelectList(_context.Syntaxes.Where(q => !q.IsHidden), "Name", "DisplayName", "text");

            Input = new InputModel
            {
                UseCaptcha = _signInManager.IsSignedIn(User),
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!_signInManager.IsSignedIn(User))
            {
                var verified = await _hCaptcha.VerifyAsync(Input.CaptchaToken, HttpContext.Connection.RemoteIpAddress);
                if (!verified)
                {
                    return Unauthorized();
                }
            }
            
            Input.AsGuest = !_signInManager.IsSignedIn(User) || Input.AsGuest;

            var paste = new Paste
            {
                Title = Input.Title ?? "Unnamed Paste",
                Cache = PasteUtils.GetShortContent(Input.Content, 250),
                Content = Input.Content,
                ExposureId = Input.ExposureId,
                SyntaxName = Input.SyntaxId,
                DateTime = DateTime.UtcNow,
                UploaderIPAddress = HttpContext.Connection.RemoteIpAddress,
                Views = 0,
            };

            string code;
            do
            {
                code = PasteUtils.GenerateRandomCode(_configuration.GetValue<int>("Paste:CodeLength"));
            } while (await _context.Pastes.AnyAsync(q => q.Code.ToLower() == code.ToLower()));

            paste.Code = code;

            if (!Input.AsGuest)
            {
                var user = await _userManager.GetUserAsync(User);
                paste.AuthorId = user.Id;
                if (Input.FolderId.HasValue)
                {
                    if (_context.Folders.Any(q => q.Id == Input.FolderId && q.OwnerId == user.Id))
                        paste.FolderId = Input.FolderId.Value;
                }
            }

            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return Redirect(code);
        }
    }
}