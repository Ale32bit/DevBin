using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DevBin.Data;
using DevBin.Models;
using DevBin.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevBin.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public BlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, Context context, AbuseIPDB abuseIPDB, IMemoryCache cache)
        {
            bool isOk = true;
            var remoteIP = httpContext.Connection.RemoteIpAddress;
            if (remoteIP != null)
            {

                if (!cache.TryGetValue("HOST:" + remoteIP.ToString(), out isOk))
                {

                    var host = await context.RemoteHosts.AsQueryable().FirstOrDefaultAsync(m => m.Address == remoteIP.GetAddressBytes());
                    if (host != null)
                    {
                        if (host.ExpireDate < DateTime.UtcNow)
                        {
                            isOk = await abuseIPDB.CheckIP(remoteIP);
                            host.Blocked = !isOk;
                            host.ExpireDate += TimeSpan.FromDays(90);
                            host.LastCheckDate = DateTime.UtcNow;

                            context.Update(host);
                            await context.SaveChangesAsync();
                        }
                        else
                        {
                            isOk = !host.Blocked;
                        }

                    }
                    else
                    {
                        isOk = await abuseIPDB.CheckIP(remoteIP);
                        host = new RemoteHost
                        {
                            Address = remoteIP.GetAddressBytes(),
                            LastCheckDate = DateTime.UtcNow,
                            Blocked = !isOk,
                        };
                        context.Add(host);
                        await context.SaveChangesAsync();
                    }


                    await cache.GetOrCreateAsync("HOST:" + remoteIP.ToString(), entry =>
                    {
                        entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                        return Task.FromResult(isOk);
                    });
                }
            }

            if (isOk)
            {
                await _next.Invoke(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await httpContext.Response.WriteAsync($"Your IP address {remoteIP} has been blacklisted from the service due to service abuse!\nFor any question contact: me - at - alexdevs - dot - me !");
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class BlacklistMiddlewareExtension
    {
        public static IApplicationBuilder UseWhitelistMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BlacklistMiddleware>();
        }
    }
}
