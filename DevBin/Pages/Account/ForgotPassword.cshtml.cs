using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly Context _context;
        private readonly Services.SendGrid _sendGrid;
        public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger, Context context, Services.SendGrid sendGrid)
        {
            _logger = logger;
            _context = context;
            _sendGrid = sendGrid;
        }

        [BindProperty]
        [EmailAddress, Required]
        public string Email { get; set; }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {

                var user = _context.Users.FirstOrDefault(q => q.Email == Email);
                if (user != null)
                {

                    if (user.PasswordResetDate.HasValue && user.PasswordResetDate > (DateTime.Now - TimeSpan.FromHours(3)))
                    {
                        ViewData["AlreadySent"] = true;
                        return Page();
                    }
                    else
                    {
                        ViewData["AlreadySent"] = false;
                    }

                    var resetCode = Utils.RandomAlphaString(64);

                    user.PasswordResetCode = resetCode;
                    user.PasswordResetDate = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                    var keys = new Dictionary<string, object>
                    {
                        { "user", user.Username },
                        { "link", $"{url}/account/resetpassword?code={resetCode}" }
                    };

                    var emailContent = Utils.GetTemplate("resetPassword", keys);
                    try
                    {
                        await _sendGrid.SendEmail(new EmailAddress(user.Email),
                            $"Password reset!",
                            emailContent,
                            emailContent
                        );
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error while sending verification mail", e);
                    }
                }
            }

            return Page();
        }
    }
}
