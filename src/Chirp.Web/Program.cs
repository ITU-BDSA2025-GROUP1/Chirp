using Chirp.Infrastructure.Data;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChirpDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")!));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ChirpDbContext>();

// Resolve DB path from env or fallback to temp
var overridePath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
var dbPath = string.IsNullOrWhiteSpace(overridePath)
    ? Path.Combine(Path.GetTempPath(), "chirp.db")
    : Path.GetFullPath(overridePath);

// Make sure directory exists
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Services
builder.Services.AddRazorPages();

// EF Core with SQLite
builder.Services.AddDbContext<ChirpDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

// App services
builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();

// Apply pending migrations automatically (or switch to EnsureCreated for a quick start)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.Migrate();

    DbInitializer.SeedDatabase(db);
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