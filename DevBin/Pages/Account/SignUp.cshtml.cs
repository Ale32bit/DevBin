using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private const string UsernameRegex = @"[A-Za-z0-9_]+";
        private readonly Context _context;
        public SignUpModel(Context context)
        {
            _context = context;
        }

        public void OnGet()
        {
        }

        #region Properties
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [PageRemote(
            ErrorMessage = "This email address is already in use.",
            AdditionalFields = "__RequestVerificationToken",
            HttpMethod = "post",
            PageHandler = "CheckEmail"
        )]
        [BindProperty]
        public string Email { get; set; }

        [Required]
        [StringLength(32, ErrorMessage = "Length must be between {2} and {1}.", MinimumLength = 3)]
        [RegularExpression(UsernameRegex, ErrorMessage = "May only contain alphanumeric characters and underscores.")]
        [Display(Name = "Username")]
        [PageRemote(
            ErrorMessage = "This username is already in use.",
            AdditionalFields = "__RequestVerificationToken",
            HttpMethod = "post",
            PageHandler = "CheckUsername"
        )]
        [BindProperty]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Must be at least {1} characters long.")]
        [MaxLength(1024, ErrorMessage = "You somehow exceeded the big length limit of 2^10. why")]
        [Display(Name = "Password")]
        [BindProperty]
        public string Password { get; set; }
        #endregion

        /*[BindProperty]
        public SignUpData SignUpData { get; set; }*/

        public async Task<IActionResult> OnPostAsync()
        {

            if (!MailAddress.TryCreate(Email, out _))
            {
                ModelState.AddModelError("Email", "Enter a valid E-Mail address.");
            }

            if (string.IsNullOrWhiteSpace(Username) || Username.Length is < 3 or > 32)
            {
                ModelState.AddModelError("Username", "Length must be between 3 and 32");
            }

            if (!Regex.IsMatch(Username!, UsernameRegex))
            {
                ModelState.AddModelError("Username", "Username may only contain alphanumeric characters and underscores.");
            }

            switch (Password.Length)
            {
                case < 8:
                    ModelState.AddModelError("Password", "Password must be at least 8 characters long.");
                    break;
                case > 1024:
                    ModelState.AddModelError("Password", "You somehow exceeded the big length limit of 2^10. why");
                    break;
            }



            if (_context.Users.Any(q => q.Email == Email))
            {
                ModelState.AddModelError("Email", "This email address is already used"); // fallback?
            }

            if (_context.Users.Any(q => q.Username == Username))
            {
                ModelState.AddModelError("Username", "This username is already used"); // fallback?
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var password = BCrypt.Net.BCrypt.EnhancedHashPassword(Password);

            var verifyCode = Utils.RandomAlphaString(64);

            var user = new User
            {
                Username = Username,
                Email = Email,
                Password = Encoding.ASCII.GetBytes(password),
                Verified = false,
                ApiToken = Utils.RandomString(128),
                VerifyCode = verifyCode,
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            await GenerateSession(_context.Users.First(q => q.Email == Email));

            return RedirectToPage("SendVerificationCode");
        }

        public JsonResult OnPostCheckEmail(string Email)
        {
            var exists = _context.Users.Any(q => q.Email == Email);

            return new JsonResult(!exists);
        }

        public JsonResult OnPostCheckUsername(string Username)
        {
            var exists = _context.Users.Any(q => q.Username == Username);

            return new JsonResult(!exists);
        }

        private async Task GenerateSession(User user)
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

            HttpContext.Response.Cookies.Append("session_token", token, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Path = "/",
                SameSite = SameSiteMode.Strict,
            });

            _context.Add(session);
            await _context.SaveChangesAsync();
        }
    }
}
