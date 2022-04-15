#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DevBin.Pages
{
    public class ReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public ReportModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public Paste Paste { get; set; }
        public string StatusMessage { get; set; }

        public IActionResult OnGet(string code)
        {
            Paste = _context.Pastes.FirstOrDefault(q => q.Code == code);
            if(Paste == null)
                return NotFound();

            return Page();
        }

        [BindProperty]
        public Report Report { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(string code)
        {
            Paste = _context.Pastes.FirstOrDefault(q => q.Code == code);
            if (Paste == null)
                return NotFound();

            Report.PasteId = Paste.Id;
            Report.ReporterIPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            if(_signInManager.IsSignedIn(User))
            {
                var reporter = await _userManager.GetUserAsync(User);
                Report.Reporter = reporter;
                Report.ReporterId = reporter.Id;
            }

            _context.Reports.Add(Report);
            await _context.SaveChangesAsync();

            var emailContent =
                        await System.IO.File.ReadAllTextAsync(Path.Join(Environment.CurrentDirectory, "Static", "Report.html"));
            emailContent = emailContent.Replace("{code}", Paste.Code);
            emailContent = emailContent.Replace("{reason}", Report.Reason);
            emailContent = emailContent.Replace("{ipaddress}", Report.ReporterIPAddress);
            emailContent = emailContent.Replace("{user}", Report.Reporter?.UserName ?? "Guest");

            await _emailSender.SendEmailAsync(_configuration["ReportEmailAddress"], "Paste report for " + Paste.Code, emailContent);

            StatusMessage = "Successfully reported!";

            return Page();
        }
    }
}
