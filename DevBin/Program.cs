global using DevBin.Models;
using DevBin.Data;
using DevBin.Services;
using DevBin.Services.HCaptcha;
using DevBin.Services.SMTP;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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
    });

#if DEBUG
builder.Services.AddSassCompiler();
#endif

var app = builder.Build();

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

app.Run();