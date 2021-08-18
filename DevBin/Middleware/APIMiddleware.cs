using DevBin.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class APIMiddleware
    {
        private readonly RequestDelegate _next;

        public APIMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, Context context)
        {
            if (httpContext.Request.Headers.TryGetValue("Authorization", out var token))
            {
                var user = context.Users.FirstOrDefault(q => q.ApiToken == token);
                if (user != null)
                {
                    httpContext.Items.TryAdd("APIUser", user);
                }
            }

            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class APIMiddlewareExtensions
    {
        public static IApplicationBuilder UseAPIMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APIMiddleware>();
        }
    }
    public class RequireTokenAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {

        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!context.HttpContext.Items.ContainsKey("APIUser"))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
