using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Chirp.Infrastructure.Data;
using Chirp.Core.Entities;

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
    public async Task GetUserTimeline_ReturnsSuccess()
    {
        // Act
        var response = await _client.GetAsync("/Helge");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UserTimeline_FirstPage_ShowsExactly32Cheeps()
    {
        // Arrange: seed a test author + 40 cheeps
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

        // request the user timeline (author name is used in route)
        var response = await _client.GetAsync($"/integration-test-author");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await response.Content.ReadAsStringAsync();

        // Count occurrences of our cheep text pattern on the page.
        var matches = Regex.Matches(html, @"\bcheep\s*\d+\b", RegexOptions.IgnoreCase);
        matches.Count.Should().Be(32);
    }
}
