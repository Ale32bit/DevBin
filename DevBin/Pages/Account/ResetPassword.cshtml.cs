using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly Context _context;
        public ResetPasswordModel(Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync([FromQuery] string? code)
        {
            var user = _context.Users.FirstOrDefault(q => q.PasswordResetCode == code);
            if (user == null)
            {
                return NotFound();
            }

            return Page();
        }

        [Required]
        [DataType(DataType.Password)]
        [BindProperty]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [BindProperty]
        public string RepeatPassword { get; set; }

        public async Task<IActionResult> OnPostAsync([FromQuery] string? code)
        {
            var user = _context.Users.FirstOrDefault(q => q.PasswordResetCode == code);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var password = BCrypt.Net.BCrypt.EnhancedHashPassword(Password);

                user.Password = Encoding.ASCII.GetBytes(password);
                user.PasswordResetCode = null;
                user.PasswordResetDate = null;

                _context.Update(user);

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("Index");
        }
    }
}
