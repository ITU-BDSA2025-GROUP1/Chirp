using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

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