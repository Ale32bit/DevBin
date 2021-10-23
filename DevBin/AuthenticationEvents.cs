using DevBin.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin
{
    public class AuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly Context _context;

        public AuthenticationEvents(Context context)
        {
            _context = context;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var userPrincipals = context.Principal;
            var user = _context.Users.FirstOrDefault(m => m.Username == userPrincipals.Identity.Name);
            if (user == null)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                context.HttpContext.Items["User"] = user;
            }
        }
    }
}
