using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DevBin.Pages.Account
{
    [RequireLogin]
    public class LogoutModel : PageModel
    {

        private readonly Context _context;

        public LogoutModel(Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = HttpContext.Items["User"];
            if (user != null)
            {
                var token = HttpContext.Request.Cookies["session_token"];
                var session = _context.Sessions.FirstOrDefault(q => q.Token == token);
                if (session != null)
                {
                    _context.Sessions.Remove(session);
                    await _context.SaveChangesAsync();
                }
            }

            return Redirect("/");
        }
    }
}
