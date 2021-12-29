using AspNetCoreRateLimit;
using DevBin.Data;
using DevBin.Middleware;
using DevBin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Sentry.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;

namespace DevBin
{
    public class Startup
    {
        private bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Services.SendGrid sendGrid = new(Configuration.GetValue<string>("SendGridToken"), Configuration.GetValue<string>("SendGridAddress"));
            var aipdb = Configuration.GetValue<string>("AbuseIPDBToken");
            AbuseIPDB abuseIPDB = new(aipdb);



            services.AddSingleton(Configuration);
            services.AddSingleton(sendGrid);
            services.AddSingleton(abuseIPDB);

            services.AddDbContext<Context>(o =>
            {
                var connString = Configuration.GetConnectionString("DefaultConnection");
                o.UseMySql(connString, ServerVersion.AutoDetect(connString));
                o.UseLazyLoadingProxies();
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
               {
                   o.EventsType = typeof(AuthenticationEvents);
               });

            services.AddScoped<AuthenticationEvents>();

            var codeLength = Configuration.GetValue<string>("PasteCodeLength");
            services.AddRazorPages(o =>
            {
                o.Conventions.AddPageRoute("/Paste", $"/{{code:length({ codeLength })}}");
            })
                .AddRazorRuntimeCompilation()
                .AddSessionStateTempDataProvider();

            services.AddControllers()
            .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .AddNewtonsoftJson();

            services.AddStackExchangeRedisCache(setup =>
            {
                setup.Configuration = Configuration.GetConnectionString("Redis");
                setup.ConfigurationOptions.ChannelPrefix = "DevBin";
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v2", new()
                {
                    Title = "DevBin",
                    Version = "v2",
                    TermsOfService = new Uri("https://devbin.dev/ToS"),
                    Contact = new OpenApiContact
                    {
                        Name = "AlexDevs",
                        Email = "me@alexdevs.me",
                        Url = new Uri("https://alexdevs.me"),
                    },
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Authorization header",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Rate Limit
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddInMemoryRateLimiting();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseForwardedHeaders(new()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

            app.UseStatusCodePages();
            app.UseStatusCodePagesWithReExecute("/Error");

            app.UseWhitelistMiddleware();
            app.UseIpRateLimiting();

            app.UseSwagger(c => { c.RouteTemplate = "docs/{documentname}/swagger.json"; });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/docs/v2/swagger.json", "DevBin API");
                c.InjectStylesheet("/swagger-ui/custom.css");
                c.DocumentTitle = "DevBin API Documentation";
                c.RoutePrefix = "docs";
            });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSentryTracing();

            app.UseAuthenticationMiddleware();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseAPIMiddleware();

            app.UseSessionMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
