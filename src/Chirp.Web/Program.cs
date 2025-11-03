using Chirp.Infrastructure.Data;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

// Detect environment as early as possible
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
var isTesting = environment.Equals("Testing", StringComparison.OrdinalIgnoreCase);

var builder = WebApplication.CreateBuilder(args);

// Resolve DB path from env or fallback to temp
var overridePath = Environment.GetEnvironmentVariable("CHIRPDBPATH");
var dbPath = string.IsNullOrWhiteSpace(overridePath)
    ? Path.Combine(Path.GetTempPath(), "chirp.db")
    : Path.GetFullPath(overridePath);

// Ensure directory exists (only needed for file-based DBs)
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

// Register services
builder.Services.AddRazorPages();

// Use SQLite for normal runs
builder.Services.AddDbContext<ChirpDbContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
});

builder.Services.AddScoped<ICheepService, CheepService>();
builder.Services.AddScoped<ICheepRepository, CheepRepository>();

var app = builder.Build();

// ✅ Only migrate and seed if NOT in testing
if (!isTesting)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
    db.Database.Migrate();
    DbInitializer.SeedDatabase(db);
}

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

// Needed for WebApplicationFactory
public partial class Program { }
