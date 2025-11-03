using Chirp.Infrastructure.Data;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// ✅ Only migrate & seed outside of testing
var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
Console.WriteLine($"Running with DOTNET_ENVIRONMENT={env}");
if (!string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();
        db.Database.Migrate();
        DbInitializer.SeedDatabase(db);
    }
}

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

public partial class Program { }
