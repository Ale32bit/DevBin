global using DevBin.Models;
using DevBin.Data;
using DevBin.Services.HCaptcha;
using DevBin.Services.SMTP;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, serverVersion);
    options.UseLazyLoadingProxies();
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddStackExchangeRedisCache(o =>
{
    o.Configuration = builder.Configuration.GetConnectionString("Redis");
    o.InstanceName = "DevBin:";
});

builder.Services.Configure<SMTPConfig>(builder.Configuration.GetSection("SMTP"));
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddSingleton<HCaptchaOptions>(new HCaptchaOptions()
{
    SiteKey = builder.Configuration["HCaptcha:SiteKey"],
    SecretKey = builder.Configuration["HCaptcha:SecretKey"],
});

builder.Services.AddScoped<HCaptcha>();

builder.Services.AddDefaultIdentity<ApplicationUser>((IdentityOptions options) =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    options.Password = new PasswordOptions
    {
        RequireDigit = true,
        RequiredLength = 8,
        RequireLowercase = false,
        RequireUppercase = false,
        RequiredUniqueChars = 1,
        RequireNonAlphanumeric = false,
    };
    options.User.RequireUniqueEmail = true;
})
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages(o =>
{
    o.Conventions.AddPageRoute("/Paste", $"/{{code:length({builder.Configuration["Paste:CodeLength"]})}}");
});

var authenticationBuilder = builder.Services.AddAuthentication()
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["Authentication:GitHub:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        o.SaveTokens = true;
    })
    /*.AddGitLab(o => {
        o.ClientId = builder.Configuration["Authentication:GitLab:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:GitLab:ClientSecret"];
    })*/
    .AddDiscord(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Discord:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];
        o.Scope.Add("identify");
        o.Scope.Add("email");
        o.SaveTokens = true;
    })
    .AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Google:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddMicrosoftAccount(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Microsoft:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
    })
    .AddSteam(o =>
    {
        o.ApplicationKey = builder.Configuration["Authentication:Steam:ApplicationKey"];
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v3", new OpenApiInfo()
    {
        Title = "DevBin v3",
        Version = "v3"
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

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

#if DEBUG
builder.Services.AddSassCompiler();
#endif

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.UseSwagger(c => { c.RouteTemplate = "docs/{documentname}/swagger.json"; });
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "DevBin";
    options.SwaggerEndpoint("/docs/v3/swagger.json", "DevBin v3");
    options.RoutePrefix = "docs";
});

using (var scope = app.Services.CreateScope())
{
    app.Logger.LogInformation("Setting up...");
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        var administratorRole = new IdentityRole<int>("Administrator");
        var result = await roleManager.CreateAsync(administratorRole);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                app.Logger.LogError($"[{error.Code}] {error.Description}");
            }
        }
    }
}

app.Run();