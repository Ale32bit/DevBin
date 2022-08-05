global using DevBin.Models;
using DevBin.Data;
using DevBin.Services.HCaptcha;
using DevBin.Services.SMTP;
using DevBin.Utils;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Prometheus;
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

var authenticationBuilder = builder.Services.AddAuthentication();

var authenticationConfig = builder.Configuration.GetSection("Authentication");
if (authenticationConfig.GetValue<bool>("GitHub:Enabled"))
{
    authenticationBuilder.AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["Authentication:GitHub:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
        o.SaveTokens = true;
    });
}

if (authenticationConfig.GetValue<bool>("Discord:Enabled"))
{
    authenticationBuilder.AddDiscord(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Discord:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Discord:ClientSecret"];
        o.Scope.Add("identify");
        o.Scope.Add("email");
        o.SaveTokens = true;
    });
}

if (authenticationConfig.GetValue<bool>("Google:Enabled"))
{
    authenticationBuilder.AddGoogle(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Google:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        o.SaveTokens = true;
    });
}

if (authenticationConfig.GetValue<bool>("Microsoft:Enabled"))
{
    authenticationBuilder.AddMicrosoftAccount(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Microsoft:ClientID"];
        o.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
        o.SaveTokens = true;
    });
}

if (authenticationConfig.GetValue<bool>("Apple:Enabled"))
{
    authenticationBuilder.AddApple(o =>
    {
        o.ClientId = builder.Configuration["Authentication:Apple:ClientID"];
        o.KeyId = builder.Configuration["Authentication:Apple:KeyID"];
        o.TeamId = builder.Configuration["Authentication:Apple:TeamID"];
        o.SaveTokens = true;

        var provider = new PhysicalFileProvider(Environment.CurrentDirectory);
        o.UsePrivateKey(keyId =>
             provider.GetFileInfo($"AuthKey_{keyId}.p8")
        );
    });
}

if (authenticationConfig.GetValue<bool>("Steam:Enabled"))
{
    authenticationBuilder.AddSteam(o =>
    {
        o.ApplicationKey = builder.Configuration["Authentication:Steam:ApplicationKey"];
        o.SaveTokens = true;
    });
}

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var openApiInfo = new OpenApiInfo()
    {
        Title = "DevBin",
        Version = "v3",
        Description = "This API provides access to the most common features of the service. A developer API token is required and must be put in the request header as \"Authorization\".",
        License = new()
        {
            Name = "GNU AGPLv3",
            Url = new("https://github.com/Ale32bit/DevBin/blob/main/LICENSE"),
        },
        Contact = new()
        {
            Name = "AlexDevs",
            Email = "devbin@alexdevs.me",
            Url = new("https://alexdevs.me"),
        },
        TermsOfService = new("https://devbin.dev/tos"),
    };
    options.SwaggerDoc("v3", openApiInfo);

    options.DocumentFilter<ApiNameNormalizeFilter>();

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

app.UseStatusCodePages();
app.UseStatusCodePagesWithReExecute("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpMetrics();

app.MapControllers();
app.MapRazorPages();
if (app.Configuration.GetValue("EnablePrometheus", false))
{
    app.MapMetrics();
}

app.UseSwagger(c => { c.RouteTemplate = "docs/{documentname}/swagger.json"; });
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "DevBin";
    options.SwaggerEndpoint("/docs/v3/swagger.json", "DevBin v3");
    options.RoutePrefix = "docs";
    options.HeadContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "wwwroot", "swagger", "menu.html"));
    options.InjectStylesheet("/lib/bootstrap/dist/css/bootstrap.css");
    options.InjectStylesheet("/lib/font-awesome/css/all.min.css");
    //options.InjectStylesheet("/css/site.css");

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

    if (!await context.Exposures.AnyAsync())
    {
        var exposures = JsonConvert.DeserializeObject<Exposure[]>(
            await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "Setup", "Exposures.json"))
        );

        if (exposures == null)
        {
            app.Logger.LogError("Could not parse Setup/Exposures.json");
            return;
        }

        foreach (var exposure in exposures.OrderBy(q => q.Id))
        {
            context.Add(exposure);
        }

        await context.SaveChangesAsync();

        app.Logger.LogInformation("Populated exposures");
    }

    if (!await context.Syntaxes.AnyAsync())
    {
        var syntaxes = JsonConvert.DeserializeObject<Syntax[]>(
            await File.ReadAllTextAsync(Path.Combine(Environment.CurrentDirectory, "Setup", "Syntaxes.json"))
        );

        if (syntaxes == null)
        {
            app.Logger.LogError("Could not parse Setup/Syntaxes.json");
            return;
        }

        foreach (var syntax in syntaxes)
        {
            context.Add(syntax);
        }

        await context.SaveChangesAsync();

        app.Logger.LogInformation("Populated syntaxes");
    }
}

app.Run();