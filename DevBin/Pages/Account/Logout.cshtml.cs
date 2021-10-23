using DevBin.Data;
using DevBin.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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
