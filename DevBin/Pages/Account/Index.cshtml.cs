using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.DTO;
using DevBin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly Context _context;
        public IndexModel(Context context)
        {
            _context = context;
        }

        [BindProperty]
        public SignInData SignInData { get; set; }

        public IActionResult OnGet()
        {
            if(HttpContext.Items.ContainsKey("User"))
            {
                return Redirect("/");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.ClearValidationState(nameof(SignInData));
            if (!TryValidateModel(SignInData, nameof(SignInData)))
            {
                return Page();
            }

            var user = _context.Users.FirstOrDefault(q => q.Username == SignInData.Username);
            if (user == null)
            {
                // TODO: Add validation error messages
                return Page();
            }

            if (Utils.ValidatePassword(user, SignInData.Password))
            {
                await GenerateSession(user, SignInData.KeepLoggedIn);

                return Redirect("/");
            }

            return Page();
        }

        private async Task GenerateSession(User user, bool keepLoggedIn = false)
        {
            string token;
            do
            {
                token = Utils.RandomString(64);
            } while (_context.Sessions.Any(q => q.Token == token));

            var session = new Session
            {
                Datetime = DateTime.Now,
                UserId = user.Id,
                Token = token,
            };

            var options = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
            };

            if (keepLoggedIn)
            {
                options.Expires = DateTime.UtcNow + TimeSpan.FromDays(60);
            }

            HttpContext.Response.Cookies.Append("session_token", token, options);

            _context.Add(session);
            await _context.SaveChangesAsync();
        }
    }
}
