using DevBin.Data;
using DevBin.Middleware;
using DevBin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevBin.Pages.Account
{
    [RequireLogin]
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
            var currentUser = HttpContext.Items["User"] as User;
            if (currentUser == null)
            {
                return Redirect("/");
            }

            if (currentUser.Verified)
            {
                return Redirect("/");
            }

            if (currentUser.VerifyCodeDate.HasValue && currentUser.VerifyCodeDate > (DateTime.Now - TimeSpan.FromHours(3)))
            {
                ViewData["AlreadySent"] = true;
                return Page();
            }
            else
            {
                ViewData["AlreadySent"] = false;
            }

            var verifyCode = Utils.RandomAlphaString(64);

            currentUser.VerificationCode = verifyCode;
            currentUser.VerifyCodeDate = DateTime.Now;
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
