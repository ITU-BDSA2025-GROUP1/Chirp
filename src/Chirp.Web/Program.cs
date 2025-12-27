// Program.cs - force SQLite and skip migrations/seeding in "Testing" environment
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
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// --- Always use SQLite: compute a writable DB path for Azure and for local dev ---
string dataDirectory;
var home = Environment.GetEnvironmentVariable("HOME"); // set by App Service
if (!string.IsNullOrEmpty(home))
{
    dataDirectory = Path.Combine(home, "data");
}
else
{
    dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
}
Directory.CreateDirectory(dataDirectory);

var dbPath = Path.Combine(dataDirectory, "chirp.db");
var sqliteConn = $"Data Source={dbPath}";

// Register the DbContext with SQLite provider (migrations assembly remains Chirp.Infrastructure)
builder.Services.AddDbContext<ChirpDbContext>(options =>
    options.UseSqlite(sqliteConn, b => b.MigrationsAssembly("Chirp.Infrastructure")));

// Identity (keeps your integer keys)
builder.Services.AddIdentity<Author, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ChirpDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

// Configure cookies (application + external)
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.SameSite = SameSiteMode.Lax;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.HttpOnly = true;
    opts.LoginPath = "/Account/Login";
    opts.LogoutPath = "/Account/Logout";
    opts.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    opts.SlidingExpiration = true;
});

// External cookie must allow cross-site for OAuth correlation
builder.Services.ConfigureExternalCookie(opts =>
{
    opts.Cookie.SameSite = SameSiteMode.None;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// App services
builder.Services.AddRazorPages();
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Chirp.Web.Services.NoOpEmailSender>();
builder.Services.AddScoped<IForgetMeService, ForgetMeService>();

// REQUIRED for Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// GitHub OAuth registration (only enabled if config present)
var githubClientId = builder.Configuration["authentication:github:clientId"] ?? string.Empty;
var githubClientSecret = builder.Configuration["authentication:github:clientSecret"] ?? string.Empty;
var githubAuthEnabled = !string.IsNullOrWhiteSpace(githubClientId) && !string.IsNullOrWhiteSpace(githubClientSecret);

var authBuilder = builder.Services.AddAuthentication();
if (githubAuthEnabled)
{
    authBuilder.AddGitHub(o =>
    {
        // ClientId/ClientSecret are non-null (empty string if not configured) so no nullable-assignment warnings
        o.ClientId = githubClientId;
        o.ClientSecret = githubClientSecret;
        o.CallbackPath = "/signin-github";
        o.Scope.Add("user:email");
    });
}

var app = builder.Build();

// Log if GitHub OAuth disabled (non-fatal)
if (!githubAuthEnabled)
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    startupLogger.LogWarning("GitHub OAuth not configured. Set authentication:github:clientId and authentication:github:clientSecret to enable external login.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Prevent aggressive caching
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseSession();

// --- Ensure DB and migrations exist on boot, but SKIP when running tests (Testing env) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var db = services.GetRequiredService<ChirpDbContext>();

        if (!app.Environment.IsEnvironment("Testing"))
        {
            // Production / Development startup: apply migrations and seed if needed
            logger.LogInformation("Applying migrations / ensuring database exists at {DbPath}", dbPath);
            db.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");

            logger.LogInformation("Seeding database (if required)...");
            DbInitializer.SeedDatabase(db); // calls Migrate() internally, but we already migrated - safe
            logger.LogInformation("Database seeding complete.");
        }
        else
        {
            // Tests will configure their own DbContext and seeding - avoid conflicts
            logger.LogInformation("Testing environment detected; skipping Program.cs migrations/seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations or seeding on startup");
        throw;
    }
}

app.MapRazorPages();
app.Run();

public partial class Program { }