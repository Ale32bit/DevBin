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
using SendGrid.Helpers.Mail;

namespace DevBin.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private readonly Context _context;
        private readonly Services.SendGrid _sendGrid;
        public SignUpModel(Context context, Services.SendGrid sendGrid)
        {
            _context = context;
            _sendGrid = sendGrid;
        }

        public void OnGet()
        {
        }

        [BindProperty]
        public SignUpData SignUpData { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.ClearValidationState(nameof(SignUpData));
            if (!TryValidateModel(SignUpData, nameof(SignUpData)))
            {
                return Page();
            }

            if (_context.Users.Any(q => q.Email == SignUpData.Email || q.Username == SignUpData.Username))
            {
                // TODO: add validation error message
                return Page();
            }

            var password = BCrypt.Net.BCrypt.EnhancedHashPassword(SignUpData.Password);

            var verifyCode = Utils.RandomAlphaString(64);

            var user = new User
            {
                Username = SignUpData.Username,
                Email = SignUpData.Email,
                Password = Encoding.ASCII.GetBytes(password),
                Verified = false,
                ApiToken = Utils.RandomString(128),
                VerifyCode = verifyCode,
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var keys = new Dictionary<string, object>
            {
                { "user", SignUpData.Username },
                { "link", $"{url}/account/verify?code={verifyCode}" }
            };

            var emailContent = Utils.GetTemplate("verify", keys);

            await _sendGrid.SendEmail(new EmailAddress(SignUpData.Email), $"Welcome to DevBin, {SignUpData.Username}!",
                content: emailContent,
                htmlContent: emailContent
                );

            await GenerateSession(_context.Users.First(q => q.Email == SignUpData.Email));

            return Redirect("/");
        }

        public JsonResult OnPostCheckEmail()
        {
            var exists = _context.Users.Any(q => q.Email == SignUpData.Email);

            return new JsonResult(!exists);
        }

        public JsonResult OnPostCheckUsername()
        {
            var exists = _context.Users.Any(q => q.Username == SignUpData.Username);

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
