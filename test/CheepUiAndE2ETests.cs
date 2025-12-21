using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace Chirp.Tests;

public class CheepUiAndE2ETests : IAsyncLifetime
{
    private IPlaywright _pw = null!;
    private IBrowser _browser = null!;
    private IPage _page = null!;
    private System.Diagnostics.Process? _serverProcess;

    // Adjust if your dev server runs on a different port
    private const string BaseUrl = "http://127.0.0.1:5273";

    public async Task InitializeAsync()
    {
        _pw = await Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
        await EnsureServerRunningAsync();
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _pw.Dispose();
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try { _serverProcess.Kill(entireProcessTree: true); } catch { /* ignore */ }
            _serverProcess.Dispose();
        }
    }

    [Fact(DisplayName = "Cheep form hidden when logged out")]
    public async Task CheepForm_Hidden_When_LoggedOut()
    {
        // ensure logged out (call logout endpoint if present, then clear cookies)
        await _page.GotoAsync(BaseUrl + "/Account/Logout").ContinueWith(_ => Task.CompletedTask);
        await _page.Context.ClearCookiesAsync();
        await _page.GotoAsync(BaseUrl + "/");
        // wait a little for UI to render
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var form = _page.Locator("#cheep-form");
        Assert.False(await form.IsVisibleAsync());
        var reminder = _page.Locator("[data-testid=\"login-reminder\"]");
        Assert.True(await reminder.IsVisibleAsync());
    }

    [Fact(DisplayName = "Cheep form visible after registration/login")]
    public async Task CheepForm_Visible_After_Login()
    {
        await _page.Context.ClearCookiesAsync();                       // ensure fresh state
        var (email, password) = await RegisterNewUserAsync();
        await LoginUserAsync(email, password);

        await _page.GotoAsync(BaseUrl + "/");
        var form = _page.Locator("form#cheep-form");
        await form.WaitForAsync(new() { Timeout = 60000 });

        var input = form.Locator("textarea[name='text'], textarea#cheep-input, input[name='text'], input#cheep-input").First;
        await input.WaitForAsync(new() { Timeout = 60000 });

        Assert.True(await form.IsVisibleAsync());
        Assert.True(await input.IsVisibleAsync());
    }

    [Fact(DisplayName = "Cheep longer than 160 chars shows validation error")]
    public async Task Cheep_TooLong_ShowsError()
    {
        await _page.Context.ClearCookiesAsync();
        var (email, password) = await RegisterNewUserAsync();
        await LoginUserAsync(email, password);

        await _page.GotoAsync(BaseUrl + "/");

        var form = _page.Locator("form#cheep-form");
        await form.WaitForAsync(new() { Timeout = 60000 });

        var input = form.Locator("textarea[name='text'], textarea#cheep-input, input[name='text'], input#cheep-input").First;
        var submit = form.Locator("button#cheep-submit, button[type=submit]").First;

        await input.FillAsync(new string('A', 161));
        await submit.ClickAsync();

        var error = _page.Locator("#cheep-error, .validation-summary-errors, .field-validation-error");
        await error.WaitForAsync(new() { Timeout = 60000 });
        var txt = await error.InnerTextAsync();
        Assert.Contains("160", txt);
    }

    [Fact(DisplayName = "Posting cheep persists and appears in timeline")]
    public async Task PostCheep_PersistsAndAppears()
    {
        await _page.Context.ClearCookiesAsync();
        var (email, password) = await RegisterNewUserAsync();
        await LoginUserAsync(email, password);

        await _page.GotoAsync(BaseUrl + "/");

        var form = _page.Locator("form#cheep-form");
        await form.WaitForAsync(new() { Timeout = 60000 });

        var input = form.Locator("textarea[name='text'], textarea#cheep-input, input[name='text'], input#cheep-input").First;
        var submit = form.Locator("button#cheep-submit, button[type=submit]").First;

        var message = "E2E Test Cheep " + Guid.NewGuid();
        await input.FillAsync(message);
        await submit.ClickAsync();

        await _page.WaitForSelectorAsync($"text={message}", new() { Timeout = 60000 });
        Assert.True(await _page.GetByText(message).IsVisibleAsync());
    }

    private async Task<(string Email, string Password)> RegisterNewUserAsync()
    {
        var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"test_{unique}@example.com";
        var password = "Passw0rd!"; // Must satisfy password policy

        await _page.GotoAsync(BaseUrl + "/Account/Register");
        await _page.FillAsync("input[name=\"Input.Name\"]", "TestUser" + unique);
        await _page.FillAsync("input[name=\"Input.Email\"]", email);
        await _page.FillAsync("input[name=\"Input.Password\"]", password);
        await _page.FillAsync("input[name=\"Input.ConfirmPassword\"]", password);

        // click the register button (use role/text fallback)
        var registerBtn = _page.Locator("button:has-text('Register'), button[type=submit]");
        await registerBtn.ClickAsync();

        // Wait for redirect/new page
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return (email, password);
    }

    private async Task LoginUserAsync(string email, string password)
    {
        await _page.GotoAsync(BaseUrl + "/Account/Login");
        await _page.FillAsync("input[name=\"Input.Email\"], input[name=\"Email\"], input[type=\"email\"]", email);
        await _page.FillAsync("input[name=\"Input.Password\"], input[name=\"Password\"], input[type=\"password\"]", password);

        // Prefer the Identity login form submit, not the GitHub button
        var loginBtn = _page.Locator("form#account button#login-submit, form#account button[type=submit], form[action*='Login'] button[type=submit]").First;
        try
        {
            await loginBtn.ClickAsync();
        }
        catch (PlaywrightException)
        {
            await _page.GetByRole(AriaRole.Button, new() { Name = "Log in", Exact = true }).ClickAsync();
        }

        await _page.WaitForSelectorAsync("#cheep-form, a:has-text('Logout'), a:has-text('Sign out')", new() { Timeout = 60000 });
    }

    private async Task EnsureServerRunningAsync()
    {
        // Probe if server already up
        try
        {
            using var client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
            var resp = await client.GetAsync(BaseUrl + "/");
            if (resp.IsSuccessStatusCode) return; // already running externally
        }
        catch { /* start below */ }

        var root = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "../../../../"));
        var webProj = System.IO.Path.Combine(root, "src", "Chirp.Web", "Chirp.Web.csproj");

        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            // build Release on demand to avoid serving stale binaries when running tests locally
            Arguments = $"run --configuration Release --project \"{webProj}\" --urls {BaseUrl}",
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        // Ensure ASPNETCORE_URLS is set for the process environment in case project uses env var
        psi.Environment["ASPNETCORE_URLS"] = BaseUrl;
        // Force Development environment so Kestrel keeps HTTP enabled (Production defaults to HTTPS-only which breaks tests)
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["DOTNET_ENVIRONMENT"] = "Development";

        var outputSb = new System.Text.StringBuilder();
        _serverProcess = System.Diagnostics.Process.Start(psi);

        if (_serverProcess == null)
            throw new Exception($"Failed to start dotnet process for project {webProj}");

        // capture output for diagnostics
        _serverProcess.OutputDataReceived += (s, e) => { if (e.Data != null) outputSb.AppendLine(e.Data); };
        _serverProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) outputSb.AppendLine(e.Data); };
        _serverProcess.BeginOutputReadLine();
        _serverProcess.BeginErrorReadLine();

        // if process exits early, include its output in the exception
        _serverProcess.EnableRaisingEvents = true;
        var tcsExited = new TaskCompletionSource<int>();
        _serverProcess.Exited += (_, __) => tcsExited.TrySetResult(_serverProcess.ExitCode);

        // Wait for readiness by polling — increase timeout to 60s
        var client2 = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(1) };
        var maxAttempts = 120; // ~60 seconds (120 * 500ms)
        for (int i = 0; i < maxAttempts; i++)
        {
            // if process exited prematurely, throw with captured output
            if (tcsExited.Task.IsCompleted)
            {
                var outText = outputSb.ToString();
                throw new Exception($"Server process exited (code {_serverProcess.ExitCode}). Output:\n{outText}");
            }

            try
            {
                await Task.Delay(500);
                var resp = await client2.GetAsync(BaseUrl + "/");
                if (resp.IsSuccessStatusCode) return;
                // accept redirect (301/302) as indication the server is up
                if ((int)resp.StatusCode >= 300 && (int)resp.StatusCode < 400) return;
            }
            catch { /* ignore and retry */ }
        }

        // timed out; include last output
        var lastOutput = outputSb.ToString();
        throw new Exception($"Server did not start within timeout for Playwright tests. Process output:\n{lastOutput}");
    }
}