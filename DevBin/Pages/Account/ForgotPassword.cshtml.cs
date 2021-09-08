using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly Context _context;
        private readonly Services.SendGrid _sendGrid;
        public ForgotPasswordModel(Context context, Services.SendGrid sendGrid)
        {
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
                    var vars = new Dictionary<string, object>();
                    vars.Add("user", user.Username);
                    //Utils.GetTemplate("resetPassword");
                }
            }

            return Page();
        }
    }
}
