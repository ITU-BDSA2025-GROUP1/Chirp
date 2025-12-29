using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

// This is a basic smoke test to verify that Playwright is set up correctly. It does not test any application functionality.
public class PlaywrightSmokeTests
{
    [Fact]
    public async Task CanOpenExampleDotCom()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://example.com");
        Assert.Contains("Example Domain", await page.TitleAsync());
    }
}