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

namespace DevBin.Pages
{
    public class ReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ReportModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
                Report.ReporterId = reporter.Id;
            }

            _context.Reports.Add(Report);
            await _context.SaveChangesAsync();

            StatusMessage = "Successfully reported!";

            return Page();
        }
    }
}
