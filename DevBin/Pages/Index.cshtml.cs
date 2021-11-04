using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;

        public IndexModel(Context context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                ViewData["Exposures"] = new SelectList(_context.Exposures, "Id", "Name");
                ViewData["ContentMaxSize"] = _configuration.GetValue<long>("PasteMaxSizes:Member");
            }
            else
            {
                ViewData["Exposures"] = new SelectList(_context.Exposures.AsQueryable().Where(q => !q.RegisteredOnly), "Id", "Name");
                ViewData["ContentMaxSize"] = ViewData["ContentMaxSize"] = _configuration.GetValue<long>("PasteMaxSizes:Guest");
            }

            MemberSpace = Utils.FriendlySize(_configuration.GetValue<int>("PasteMaxSizes:Member"));

            var syntaxes = await _context.Syntaxes.AsQueryable().Where(q => q.Show && q.Id != 0).OrderBy(q => q.Pretty).ToListAsync();
            var autoDetect = await _context.Syntaxes.AsQueryable().FirstOrDefaultAsync(q => q.Id == 0);
            if(autoDetect != null)
            {
                syntaxes = syntaxes.Prepend(autoDetect).ToList();
            }

            ViewData["Syntaxes"] = new SelectList(syntaxes, "Id", "Pretty", autoDetect != null ? 0 : 1);
            UserPaste = new() { SyntaxId = autoDetect != null ? 0 : 1 };

            if (HttpContext.Request.Query.ContainsKey("clone"))
            {
                var code = HttpContext.Request.Query["clone"].ToString();
                var paste = await _context.Pastes.AsQueryable().FirstOrDefaultAsync(q => q.Code == code);
                if (paste == null)
                {
                    return NotFound();
                }

                if (HttpContext.User.Identity != null && paste.Exposure.RegisteredOnly && HttpContext.User.Identity.IsAuthenticated)
                {
                    var currentUser = await _context.Users.AsQueryable().FirstOrDefaultAsync(q => q.Email == paste.Author.Email);
                    if (currentUser == null)
                    {
                        return Unauthorized();
                    }
                }

                UserPaste = new UserPasteForm
                {
                    Content = paste.Content,
                    SyntaxId = paste.SyntaxId,
                };
            }
            else
            {
                UserPaste = new();

                if (HttpContext.Request.Query.TryGetValue("text", out var shareContent))
                {
                    UserPaste.Content = shareContent.ToString();
                }
                if (HttpContext.Request.Query.TryGetValue("title", out var shareTitle))
                {
                    UserPaste.Title = shareTitle.ToString();
                }
                if (HttpContext.Request.Query.TryGetValue("url", out var shareUrl))
                {
                    UserPaste.Content += "\n" + shareUrl.ToString();
                }
            }

            return Page();
        }

        public Paste Paste { get; set; }
        [BindProperty]
        public UserPasteForm UserPaste { get; set; }
        
        public string MemberSpace { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            long pasteMaxSize;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                pasteMaxSize = _configuration.GetSection("PasteMaxSizes").GetValue<long>("Member");
            }
            else
            {
                pasteMaxSize = _configuration.GetSection("PasteMaxSizes").GetValue<long>("Guest");
            }

            if (UserPaste.Content.Length > pasteMaxSize)
            {
                ModelState.AddModelError("UserPaste.Content", "The paste content is too big!");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // User input checks
            if (!_context.Exposures.Any(q => q.Id == UserPaste.ExposureId))
            {
                UserPaste.ExposureId = _context.Exposures.First().Id;
            }

            if (!_context.Syntaxes.Any(q => q.Id == UserPaste.SyntaxId))
            {
                UserPaste.SyntaxId = _context.Syntaxes.First().Id;
            }

            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var exposure = _context.Exposures.First(q => q.Id == UserPaste.ExposureId);
                if (exposure.RegisteredOnly)
                {
                    return Unauthorized();
                }
            }

            Paste = new Paste
            {
                Title = UserPaste.Title ?? "Unnamed Paste",
                SyntaxId = UserPaste.SyntaxId ?? 1,
                ExposureId = UserPaste.ExposureId,
                Content = UserPaste.Content ?? "",
                AuthorId = null,
                Author = null,
            };


            if (User.Identity is { IsAuthenticated: true } && !UserPaste.AsGuest)
            {
                var author = HttpContext.Items["User"] as User;
                if (author != null)
                {
                    Paste.Author = author;
                    Paste.AuthorId = author.Id;
                }
            }

            do
            {
                Paste.Code = Utils.RandomAlphaString(_configuration.GetValue<int>("PasteCodeLength"));
            } while (_context.Pastes.Any(q => q.Code == Paste.Code));

            Paste.Datetime = DateTime.UtcNow;
            Paste.Cache = Paste.Content[..Math.Min(Paste.Content.Length, 255)];
            Paste.IpAddress = HttpContext.Connection.RemoteIpAddress.GetAddressBytes();

            _context.Pastes.Add(Paste);
            await _context.SaveChangesAsync();

            return Redirect("/" + Paste.Code);
        }
    }
}
