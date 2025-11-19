using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using Chirp.Web;

namespace Chirp.Tests;

public class WebAppFactory : WebApplicationFactory<global::Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Testing" so Program.cs skips migrations/seeding
        builder.UseEnvironment("Testing");

        builder.ConfigureLogging(logging =>
        {
            // Remove EF Core info logs that flood the console
            logging.ClearProviders();
            logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ChirpDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Create a single in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ChirpDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.EnableSensitiveDataLogging(false);
            });

            // Build service provider and initialize the DB
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChirpDbContext>();

            // Ensure schema is created safely
            db.Database.EnsureCreated();

            // Seed the DB
            //"comment"
            DbInitializer.SeedTestDataBase(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
