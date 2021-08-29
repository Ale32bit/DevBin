using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    [RequireLogin]
    public class SettingsModel : PageModel
    {
        private readonly Context _context;
        public SettingsModel(Context context)
        {
            _context = context;
        }

        #region Properties

        [EmailAddress]
        [BindProperty]
        public string EmailChange { get; set; }

        [DataType(DataType.Password)]
        [BindProperty]
        public string CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        [BindProperty]
        public string NewPassword { get; set; }
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        [BindProperty]
        public string NewRepeatPassword { get; set; }

        [BindProperty]
        public string APIToken { get; set; }

        [DataType(DataType.Password)]
        [BindProperty]
        public string DeletionPassword { get; set; }

        #endregion

        public void OnGet()
        {
            var user = HttpContext.Items["User"] as Models.User;

            EmailChange = user.Email;
            APIToken = user.ApiToken;
        }

        public async Task<IActionResult> OnPostUpdateEmailAsync()
        {
            var user = HttpContext.Items["User"] as Models.User;
            if (!MailAddress.TryCreate(EmailChange, out _))
            {
                ModelState.AddModelError("EmailChange", "Enter a valid E-Mail address.");
            }

            if(_context.Users.Any(q => q.Email == EmailChange))
            {
                ModelState.AddModelError("EmailChange", "This E-Mail address is already used.");
            }

            if (ModelState.IsValid)
            {
                user.Email = EmailChange;
                user.Verified = false;

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToPage("SendVerificationCode");

            }

            APIToken = user.ApiToken;

            return Page();
        }
    }
}
