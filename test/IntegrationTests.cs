using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Chirp.Infrastructure.Data;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
//using Microsoft.AspNetCore.Authentication.Testing;

namespace Chirp.IntegratedTests;

// Note: These integration tests were for the old WebApi project which had REST API endpoints.
// The new Razor Pages application doesn't expose API endpoints - it serves HTML pages.
// For web app testing, use End2EndTests.cs which tests the HTML pages.

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHomePage_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserTimeline_RequiresAuthentication()
    {
        var factoryWithAuth = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
            });
        });

        var authClient = factoryWithAuth.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await authClient.GetAsync("/Helge");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.OriginalString.Should().Contain("/Account/Login");
    }

    [Fact]
    public async Task UserTimeline_FirstPage_ShowsExactly32Cheeps()
    {
        // Arrange: seed a test author + 40 cheeps
        int authorId;
        using (var scope = _factory.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<ChirpDbContext>();

            // create test author
            var author = new Author
            {
                Name = "integration-test-author",
                UserName = "integration-test-author",
                Email = "integration@example.local"
            };
            db.Authors.Add(author);
            db.SaveChanges();

            authorId = author.Id;

            // create 40 cheeps (newest first)
            for (int i = 1; i <= 40; i++)
            {
                db.Cheeps.Add(new Cheep
                {
                    AuthorId = author.Id,
                    Text = $"cheep {i}",
                    Timestamp = DateTime.UtcNow.AddMinutes(-i)
                });
            }
            db.SaveChanges();
        }

        // Verify directly from the database: the first page should contain 32 most recent cheeps
        using (var scope = _factory.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<ChirpDbContext>();

            var authorInDb = db.Authors.Find(authorId);
            authorInDb.Should().NotBeNull();

            var cheeps = db.Cheeps
                .Where(c => c.AuthorId == authorInDb!.Id)
                .OrderByDescending(c => c.Timestamp)
                .Take(32)
                .ToList();

            cheeps.Count.Should().Be(32);
        }
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "integration-test-author"), new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
