using DevBin.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace DevBin.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, Context context)
        {
            if (!httpContext.Request.Cookies.TryGetValue("session_token", out var token))
                return _next.Invoke(httpContext);

            if (string.IsNullOrEmpty(token))
                return _next.Invoke(httpContext);

            var session = context.Sessions.FirstOrDefault(q => q.Token == token);
            if (session != null)
            {
                var userData = session.User;

                var user = new GenericIdentity(userData.Email);
                var principal = new GenericPrincipal(user, null);
                httpContext.User = principal;
                httpContext.Items.Add("User", userData);

                Thread.CurrentPrincipal = principal;
            }
            return _next.Invoke(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}
