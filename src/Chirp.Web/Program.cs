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
// configure cookie for dev if needed (do not add duplicate auth schemes)
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // dev only
    opts.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    opts.Cookie.HttpOnly = true;
});

// Services
builder.Services.AddRazorPages();

// App services
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();
// register your implementation explicitly to avoid ambiguity
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Chirp.Web.Services.NoOpEmailSender>();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();

// Make the implicit Program class public for testing
public partial class Program { }