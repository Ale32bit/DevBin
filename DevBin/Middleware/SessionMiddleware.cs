using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevBin.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            var origin = httpContext.Request.Headers["User-Agent"] +
                         httpContext.Connection.RemoteIpAddress +
                         DateTime.Today;
            if (httpContext.User.Identity is {IsAuthenticated: true})
            {
                origin += httpContext.User.Identity.Name;
            }
            var sessionIdByte =
                SHA256.HashData(Encoding.ASCII.GetBytes(origin));

            var sessionId = BitConverter.ToString(sessionIdByte).Replace("-", "");
            httpContext.Items.TryAdd("SessionId", sessionId);

            
            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class SessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionMiddleware>();
        }
    }
}
