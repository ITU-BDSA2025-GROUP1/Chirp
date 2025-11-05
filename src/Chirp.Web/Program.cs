using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- single, explicit DB registration with migrations assembly set to Chirp.Infrastructure ---
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "chirp.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<ChirpDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}", b => b.MigrationsAssembly("Chirp.Infrastructure"));
});

// register Identity with int keys
builder.Services.AddIdentity<Author, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
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

// apply migrations on the same DB file/DbContext
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.Migrate();
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