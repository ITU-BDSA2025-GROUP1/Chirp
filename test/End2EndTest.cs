using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Chirp.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Chirp.E2E;




[Collection("SharedFactory")]
public class End2EndTests
{
    private readonly HttpClient _client;
    private readonly WebAppFactory _factory;
    private readonly ITestOutputHelper _output;

    public End2EndTests(WebAppFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact(DisplayName = "Public timeline HTML contains Helge's cheep")]
    public async Task PublicTimeline_HtmlContainsHelgeCheep()
    {
        var resp = await _client.GetAsync("/?page=21");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var html = await resp.Content.ReadAsStringAsync();
        html.Should().Contain("Helge");
        html.Should().Contain("Hello, BDSA students!");
    }

    [Fact(DisplayName = "Adrian's timeline redirects to login when anonymous")]
    public async Task AdrianTimeline_RequiresLogin()
    {
        var authClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var resp = await authClient.GetAsync("/Adrian");
        resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
        resp.Headers.Location.Should().NotBeNull();
        resp.Headers.Location!.OriginalString.Should().Contain("/Account/Login");
    }

    [Fact(DisplayName = "Public timeline returns OK and HTML content")]
    public async Task PublicTimeline_ReturnsHtml()
    {
        var resp = await _client.GetAsync("/");
        
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var contentType = resp.Content.Headers.ContentType?.MediaType;
        contentType.Should().Be("text/html");
        
        var html = await resp.Content.ReadAsStringAsync();
        html.Should().NotBeNullOrWhiteSpace();
        html.Should().Contain("Chirp!");
    }

    [Fact(DisplayName = "User timeline redirects to login when anonymous")]
    public async Task UserTimeline_RequiresLogin()
    {
        var authClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var resp = await authClient.GetAsync("/Helge?page=1");

        resp.StatusCode.Should().Be(HttpStatusCode.Redirect);
        resp.Headers.Location.Should().NotBeNull();
        resp.Headers.Location!.OriginalString.Should().Contain("/Account/Login");
    }
}