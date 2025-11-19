using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // for GetRequiredService
using Xunit;
using Chirp.Tests; // for WebAppFactory

namespace Chirp.Tests;

[Collection("SharedFactory")]
public class ExternalAuthConfigurationTests
{
    private readonly WebAppFactory _factory;

    public ExternalAuthConfigurationTests(WebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "GitHub auth scheme only registered when secrets present")]
    public async Task GitHubScheme_ConditionalRegistration_Works()
    {
        var services = _factory.Services;
        var cfg = services.GetRequiredService<IConfiguration>();
        var schemeProvider = services.GetRequiredService<IAuthenticationSchemeProvider>();

        var clientId = cfg["authentication:github:clientId"]; // null/empty in CI without secrets
        var clientSecret = cfg["authentication:github:clientSecret"]; // null/empty in CI without secrets
        var secretsPresent = !string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret);

        var githubScheme = await schemeProvider.GetSchemeAsync("GitHub");

        if (secretsPresent)
        {
            githubScheme.Should().NotBeNull("GitHub scheme should be registered when secrets are provided");
        }
        else
        {
            githubScheme.Should().BeNull("GitHub scheme should not be registered when secrets are missing");
        }
    }
}
