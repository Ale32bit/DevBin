using DevBin.Data;
using DevBin.DTO;
using DevBin.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

        [Required]
        [StringLength(32, ErrorMessage = "Length must be between {2} and {1}.", MinimumLength = 3)]
        [RegularExpression(@"[A-Za-z0-9_]+", ErrorMessage = "May only contain alphanumeric characters and underscores.")]
        [Display(Name = "Username")]
        [BindProperty]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Must be at least {1} characters long.")]
        [MaxLength(1024, ErrorMessage = "You somehow exceeded the big length limit of 2^10. why")]
        [Display(Name = "Password")]
        [BindProperty]
        public string Password { get; set; }

        [Display(Name = "Keep me logged in")]
        [BindProperty]
        public bool KeepLoggedIn { get; set; } = false;

        public IActionResult OnGet()
        {
            if (HttpContext.Items.ContainsKey("User"))
            {
                return RedirectToPage("Settings");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await AuthenticateUser(Username, Password);

            if (user == null)
            {
                ModelState.AddModelError("Password", "Incorrect username or password.");
            }

            if (ModelState.IsValid)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = KeepLoggedIn,
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                //await GenerateSession(user, KeepLoggedIn);

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

        private async Task<User> AuthenticateUser(string username, string password)
        {
            var user = await _context.Users.AsQueryable().FirstOrDefaultAsync(q => q.Username == username);
            if (user != null)
            {
                if (Utils.ValidatePassword(user, password))
                {
                    return user;
                }
            }

            return null;
        }
    }
}
