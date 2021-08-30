using DevBin.Data;
using DevBin.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

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

            if (_context.Users.Any(q => q.Email == EmailChange))
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

        public async Task<IActionResult> OnPostUpdatePasswordAsync()
        {
            var user = HttpContext.Items["User"] as Models.User;

            if (string.IsNullOrEmpty(NewPassword))
            {
                ModelState.AddModelError("NewPassword", "Password must be at least 8 characters long.");
            }
            else
            {

                switch (NewPassword.Length)
                {
                    case < 8:
                        ModelState.AddModelError("NewPassword", "Password must be at least 8 characters long.");
                        break;
                    case > 1024:
                        ModelState.AddModelError("NewPassword", "You somehow exceeded the big length limit of 2^10. why");
                        break;
                }
            }

            if (ModelState.IsValid)
            {

                var newPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(NewPassword);
                user.Password = Encoding.ASCII.GetBytes(newPassword);

                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToPage("");
            }

            return Page();
        }

        public async Task<IActionResult> OnGetGenerateKeyAsync()
        {
            var user = HttpContext.Items["User"] as Models.User;

            user.ApiToken = Utils.RandomString(48);
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("");
        }

        public async Task<IActionResult> OnGetDeleteKeyAsync()
        {
            var user = HttpContext.Items["User"] as Models.User;

            user.ApiToken = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("");
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var user = HttpContext.Items["User"] as Models.User;

            if (!Utils.ValidatePassword(user, DeletionPassword))
            {
                ModelState.AddModelError("DeletionPassword", "Password is incorrect.");
            }

            if (ModelState.IsValid)
            {
                // Delete all pastes of the user
                var userPastes = _context.Pastes.Where(q => q.AuthorId == user.Id);
                _context.Pastes.RemoveRange(userPastes);

                // Delete all sessions of the user
                var sessions = _context.Sessions.Where(q => q.UserId == user.Id);
                _context.Sessions.RemoveRange(sessions);

                // Delete the user
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                return Redirect("/");

            }

            return Page();
        }
    }
}
