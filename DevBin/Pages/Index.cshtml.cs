using DevBin.Data;
using DevBin.Services.HCaptcha;
using DevBin.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Localization;

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
        private readonly IStringLocalizer _localizer;
        private readonly IStringLocalizer _shared;

        public IndexModel(
            ILogger<IndexModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            HCaptcha hCaptcha,
            IStringLocalizer<IndexModel> localizer,
            IStringLocalizer<_Shared> shared)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _hCaptcha = hCaptcha;
            _localizer = localizer;
            _shared = shared;

            Latest = _context.Pastes.Where(q => q.Exposure.IsListed).OrderByDescending(q => q.DateTime).Take(3)
                .ToList();
            PasteSpace = User != null && _signInManager.IsSignedIn(User)
                ? _configuration.GetValue<int>("Paste:MaxContentSize:Member")
                : _configuration.GetValue<int>("Paste:MaxContentSize:Guest", 1024 * 2);
            MemberSpace = Utils.Utils.ToIECFormat(_configuration.GetValue<int>("Paste:MaxContentSize:Member"));
            Alerts = _configuration.GetSection("Alerts").Get<Alert[]>();
        }

        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.MultilineText)]
            public string Content { get; set; }

            [DataType(DataType.Text)] public string? Title { get; set; }
            [Required] public string SyntaxName { get; set; }
            [Required] public int ExposureId { get; set; }
            public bool AsGuest { get; set; }
            public int? FolderId { get; set; }

            public string? CaptchaToken { get; set; }
            public bool UseCaptcha { get; set; }
        }

        [BindProperty] public IList<Paste> Latest { get; set; }

        [BindProperty] public int PasteSpace { get; set; }

        [BindProperty] public string MemberSpace { get; set; }

        public bool IsEditing { get; set; }
        public Alert[] Alerts { get; set; }

        public async Task OnGetAsync()
        {
            var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture;
            _logger.LogDebug($"User locale is {culture.Name}");

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

            ViewData["Exposures"] = new SelectList(exposures.Select(q => new
            {
                Id = q.Id,
                Name = _shared["Exposure." + q.Name],
            }), "Id", "Name", 1);

            ViewData["Syntaxes"] = new SelectList(_context.Syntaxes.Where(q => !q.IsHidden && q.Name != "auto"), "Name", "DisplayName", "auto");

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

            if (Input.Content == null)
            {
                ModelState.AddModelError("Input.Content", _localizer["Error.Content.Empty"]);
                return Page();
            }

            if (Input.Content.Length > PasteSpace)
            {
                ModelState.AddModelError("Input.Content", _localizer["Error.Content.ExceededLength"]);
                return Page();
            }

            var paste = new Paste
            {
                Title = Input.Title ?? "Unnamed Paste",
                Cache = PasteUtils.GetShortContent(Input.Content, 250),
                Content = Encoding.UTF8.GetBytes(Input.Content),
                ExposureId = Input.ExposureId,
                SyntaxName = Input.SyntaxName,
                DateTime = DateTime.UtcNow,
                UploaderIPAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
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

        public async Task<IActionResult> OnGetEditAsync(string code)
        {
            if (!_signInManager.IsSignedIn(User))
                return Unauthorized();

            if (code == null)
                return NotFound();

            var paste = await _context.Pastes.FirstOrDefaultAsync(q => q.Code == code);
            if (paste == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (paste.AuthorId != user.Id)
                return Unauthorized();

            ViewData["Folders"] = new SelectList(_context.Folders.Where(q => q.OwnerId == user.Id), "Id", "Name",
                paste.FolderId);
            ViewData["Exposures"] = new SelectList(_context.Exposures.Select(q => new
            {
                Id = q.Id,
                Name = _shared["Exposure." + q.Name],
            }), "Id", "Name", paste.ExposureId);
            ViewData["Syntaxes"] = new SelectList(_context.Syntaxes.Where(q => !q.IsHidden && q.Name != "auto"), "Name",
                "DisplayName", paste.SyntaxName);

            Input = new InputModel
            {
                UseCaptcha = false,
                Title = paste.Title,
                ExposureId = paste.ExposureId,
                SyntaxName = paste.SyntaxName,
                FolderId = paste.FolderId,
                Content = paste.StringContent,
            };

            IsEditing = true;
            return Page();
        }

        public async Task<IActionResult> OnPostEditAsync(string code)
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

            if (Input.Content == null)
            {
                ModelState.AddModelError("Input.Content", _localizer["Error.Content.Empty"]);
                return Page();
            }

            if (Input.Content.Length > PasteSpace)
            {
                ModelState.AddModelError("Input.Content", _localizer["Error.Content.ExceededLength"]);
                return Page();
            }

            paste.Title = Input.Title;
            paste.SyntaxName = Input.SyntaxName;
            paste.ExposureId = Input.ExposureId;
            paste.Content = Encoding.UTF8.GetBytes(Input.Content);
            if (Input.FolderId != 0)
            {
                paste.FolderId = Input.FolderId;
            }

            paste.UpdateDatetime = DateTime.UtcNow;

            paste.Cache = PasteUtils.GetShortContent(paste.StringContent, 250);

            _context.Update(paste);
            await _context.SaveChangesAsync();

            return Redirect(code);
        }

        public async Task<IActionResult> OnGetCloneAsync(string code)
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

            var exposures = _context.Exposures.AsQueryable();
            if (!_signInManager.IsSignedIn(User))
            {
                exposures = exposures.Where(q => !q.IsAuthorOnly);
            }

            ViewData["Exposures"] = new SelectList(exposures.Select(q => new
            {
                Id = q.Id,
                Name = _shared["Exposure." + q.Name],
            }), "Id", "Name", 1);
            ViewData["Syntaxes"] = new SelectList(_context.Syntaxes.Where(q => !q.IsHidden && q.Name != "auto"), "Name",
                "DisplayName", paste.SyntaxName);

            Input = new InputModel
            {
                UseCaptcha = _signInManager.IsSignedIn(User),
                Content = paste.StringContent,
                SyntaxName = paste.SyntaxName,
            };

            return Page();
        }
    }
}