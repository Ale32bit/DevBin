using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace DevBin.Pages.Account
{
    public class SendVerificationCodeModel : PageModel
    {
        private readonly Context _context;
        private readonly Services.SendGrid _sendGrid;
        private readonly ILogger<SendVerificationCodeModel> _logger;

        public SendVerificationCodeModel(Context context, Services.SendGrid sendGrid, ILogger<SendVerificationCodeModel> logger)
        {
            _context = context;
            _sendGrid = sendGrid;
            _logger = logger;
        }
        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
            {
                return Redirect("/");
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(q => q.Email == HttpContext.User.Identity.Name);

            if (currentUser.Verified)
            {
                return Redirect("/");
            }

            var verifyCode = Utils.RandomAlphaString(64);

            currentUser.VerifyCode = verifyCode;
            _context.Update(currentUser);
            await _context.SaveChangesAsync();

            var url = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            var keys = new Dictionary<string, object>
            {
                { "user", currentUser.Username },
                { "link", $"{url}/account/verify?code={verifyCode}" }
            };

            var emailContent = Utils.GetTemplate("verify", keys);
            try
            {
                await _sendGrid.SendEmail(new EmailAddress(currentUser.Email),
                    $"Welcome to DevBin, {currentUser.Username}!",
                    emailContent,
                    emailContent
                );
            }
            catch (Exception e)
            {
                _logger.LogError("Error while sending verification mail", e);
            }

            return Page();
        }
    }
}
