using Chirp.Infrastructure.Data;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Chirp.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Read optional connection string (used if you switch to SQL Server in production).
var connectionString = builder.Configuration.GetConnectionString("ChirpDbContextConnection");

// Choose a writable location for SQLite in Production (App Service 'HOME' is writable when
// the app is deployed as Run From Package). Fallback to ContentRoot/App_Data if HOME isn't set.
string dataDirectory;
if (builder.Environment.IsDevelopment())
{
    dataDirectory = builder.Environment.ContentRootPath;
}
else
{
    var home = Environment.GetEnvironmentVariable("HOME");
    dataDirectory = !string.IsNullOrWhiteSpace(home)
        ? Path.Combine(home, "data")
        : Path.Combine(builder.Environment.ContentRootPath, "App_Data");
}
Directory.CreateDirectory(dataDirectory);

// Single DbContext registration (point to the same DB and migrations assembly)
var dbPath = Path.Combine(dataDirectory, "chirp.db");
builder.Services.AddDbContext<ChirpDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}", b => b.MigrationsAssembly("Chirp.Infrastructure"))
);

// single identity registration — remove any other AddIdentity/AddDefaultIdentity calls
builder.Services.AddIdentity<Author, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ChirpDbContext>()
.AddDefaultTokenProviders();
    
builder.Services.Configure<IdentityOptions>(options =>
{
    // allow email characters in usernames
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
// configure Application and External cookies for OAuth state to work across sites
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.SameSite = SameSiteMode.None;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.LoginPath = "/Account/Login";
    opts.LogoutPath = "/Account/Logout";
});

// critical: external cookie (used to hold the OAuth state) must allow cross-site
builder.Services.ConfigureExternalCookie(opts =>
{
    opts.Cookie.SameSite = SameSiteMode.None;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Chirp.Web.Services.NoOpEmailSender>();

// add session if you use app.UseSession()
builder.Services.AddSession();

// ------ Move this BEFORE builder.Build() ------
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "GitHub";
    })
    .AddCookie()
    .AddGitHub(o =>
    {
        // These values should be provided via configuration (App Service application settings).
        o.ClientId = builder.Configuration["authentication:github:clientId"]!;
        o.ClientSecret = builder.Configuration["authentication:github:clientSecret"]!;
        o.CallbackPath = "/signin-github";
    });
// --------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Ensure database is created/migrated and seeded on startup (important for first-run on Azure App Service)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    try
    {
        Chirp.Infrastructure.Data.DbInitializer.SeedDatabase(db);
    }
    catch (Exception ex)
    {
        // Log and rethrow to surface startup errors in App Service logs
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogError(ex, "Failed to migrate/seed the database at path {DbPath}", dbPath);
        throw;
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.Run();

public partial class Program { }
