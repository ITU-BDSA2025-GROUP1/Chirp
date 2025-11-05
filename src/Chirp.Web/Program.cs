using Chirp.Infrastructure.Data;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ChirpDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ChirpDbContextConnection' not found.");;

// Single DbContext registration (point to the same DB and migrations assembly)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "chirp.db");
builder.Services.AddDbContext<ChirpDbContext>(o =>
    o.UseSqlite($"Data Source={dbPath}", b => b.MigrationsAssembly("Chirp.Infrastructure")));

builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ChirpDbContext>();

// Identity with int keys
builder.Services.AddIdentity<Author, IdentityRole<int>>(o =>
{
    o.SignIn.RequireConfirmedAccount = false;
    o.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ChirpDbContext>()
.AddDefaultTokenProviders();
    
// configure cookie for dev (if needed)
builder.Services.ConfigureApplicationCookie(opts =>
{
    opts.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None; // dev only
    opts.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
});

// Services
builder.Services.AddRazorPages();

// App services
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();

// Ensure DB is created/migrated and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.Migrate(); // or EnsureCreated() if you don't use migrations
    if (!db.Cheeps.Any())
    {
        DbInitializer.SeedDatabase(db);
    }
}

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

// Make the implicit Program class public for testing
public partial class Program { }