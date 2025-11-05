using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Chirp.WebApi;

// Note: These tests were for the old WebApi project which has been replaced
// by a Razor Pages web application in the new Onion Architecture.
// The API-specific tests are now obsolete and have been removed.
// Web application tests should use End2EndTests.cs with WebApplicationFactory.

//Comment to be deleted

public class WebApiTests
{
    [Fact]
    public void PlaceholderTest_ForFutureWebAppTests()
    {
        // This is a placeholder test.
        // Add new web application-specific tests in End2EndTests.cs
        Assert.True(true);
    }
}