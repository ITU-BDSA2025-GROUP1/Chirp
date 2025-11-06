using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Chirp.IntegratedTests;

// Note: These integration tests were for the old WebApi project which had REST API endpoints.
// The new Razor Pages application doesn't expose API endpoints - it serves HTML pages.
// For web app testing, use End2EndTests.cs which tests the HTML pages.

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
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
}
