using System;
using Castle.Core.Logging;
using DevBin.Data;
using DevBin.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevBin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            PasteStore pasteStore = new(Configuration.GetValue<string>("PastesPath"));

            services.AddSingleton(Configuration);
            services.AddSingleton(pasteStore);

            services.AddDbContext<Context>(o =>
            {
                var connString = Configuration.GetConnectionString("DefaultConnection");
                o.UseMySql(connString, ServerVersion.AutoDetect(connString));
                o.UseLazyLoadingProxies();
            });

            var codeLength = Configuration.GetValue<string>("PasteCodeLength");
            services.AddRazorPages(o =>
            {
                o.Conventions.AddPageRoute("/Paste", $"/{{code:length({ codeLength })}}");
            })
                .AddRazorRuntimeCompilation()
                .AddSessionStateTempDataProvider();

            services.AddMemoryCache();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthenticationMiddleware();
            app.UseAuthorization();

            app.UseSessionMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
