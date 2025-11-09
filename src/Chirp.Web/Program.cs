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

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ChirpDbContextConnection") 
    ?? throw new InvalidOperationException("Connection string 'ChirpDbContextConnection' not found.");

// Single DbContext registration (point to the same DB and migrations assembly)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "chirp.db");
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
    opts.Cookie.SameSite = SameSiteMode.Lax;
    opts.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opts.Cookie.HttpOnly = true;
    opts.LoginPath = "/Account/Login";
    opts.LogoutPath = "/Account/Logout";
    opts.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    opts.SlidingExpiration = true;
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

// Add GitHub authentication - Identity already provides cookie authentication
builder.Services.AddAuthentication()
    .AddGitHub(o =>
    {
        o.ClientId = builder.Configuration["authentication:github:clientId"];
        o.ClientSecret = builder.Configuration["authentication:github:clientSecret"];
        o.CallbackPath = "/signin-github";
        
        // Request email scope from GitHub
        o.Scope.Add("user:email");
    });
// --------------------------------------------

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Add middleware to prevent caching after authentication is set up
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseSession();
app.MapRazorPages();
app.Run();

public partial class Program { }
