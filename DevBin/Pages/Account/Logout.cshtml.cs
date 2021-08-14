using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevBin.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    public class LogoutModel : PageModel
    {

        private readonly Context _context;

        public LogoutModel(Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.User.Identity!.IsAuthenticated)
            {
                var currentUser = _context.Users.FirstOrDefault(q => q.Email == HttpContext.User.Identity.Name);
                if(currentUser != null)
                {
                    var token = HttpContext.Request.Cookies["session_token"];
                    var session = _context.Sessions.FirstOrDefault(q => q.Token == token);
                    if(session != null)
                    {
                        _context.Sessions.Remove(session);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Redirect("/");
        }
    }
}
