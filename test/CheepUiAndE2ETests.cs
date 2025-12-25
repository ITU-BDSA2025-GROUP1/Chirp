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

    private static string GetFreeUrl()
{
    var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
    listener.Start();
    int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();
    return $"http://localhost:{port}";
}
    private string BaseUrl = GetFreeUrl();

    public async Task InitializeAsync()
    {
        _pw = await Playwright.CreateAsync();
        _browser = await _pw.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _page = await _browser.NewPageAsync();
        await EnsureDatabaseCreatedAsync();
        await EnsureServerRunningAsync();
    }

    private async Task EnsureDatabaseCreatedAsync()
{
    var root = System.IO.Path.GetFullPath(System.IO.Path. Combine(AppContext.BaseDirectory, "../../../../"));
    var webProj = System.IO.Path.Combine(root, "src", "Chirp.Web");
    
    // Run migrations to create/update database schema
    var psi = new System.Diagnostics.ProcessStartInfo
    {
        FileName = "dotnet",
        Arguments = "ef database update",
        WorkingDirectory = webProj,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    
    using var process = System.Diagnostics.Process.Start(psi);
    if (process != null)
    {
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new Exception($"Database migration failed with exit code {process.ExitCode}");
        }
    }
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
        await _page.GotoAsync(BaseUrl + "/");
        var form = _page.Locator("#cheep-form");
        Assert.False(await form.IsVisibleAsync());
        var reminder = _page.GetByTestId("login-reminder");
        Assert.True(await reminder.IsVisibleAsync());
    }

    [Fact(DisplayName = "Cheep form visible after registration/login")]
    public async Task CheepForm_Visible_After_Login()
    {
        await RegisterNewUserAsync();
        await _page.GotoAsync(BaseUrl + "/");
        Assert.True(await _page.Locator("#cheep-form").IsVisibleAsync());
        Assert.True(await _page.GetByTestId("cheep-input").IsVisibleAsync());
    }

    [Fact(DisplayName = "Cheep longer than 160 chars shows validation error")]
    public async Task Cheep_TooLong_ShowsError()
    {
        await RegisterNewUserAsync();
        await _page.GotoAsync(BaseUrl + "/");
        var longText = new string('A', 161);
        await _page.GetByTestId("cheep-input").FillAsync(longText);
        await _page.GetByTestId("cheep-submit").ClickAsync();
        // Stay on page (no redirect) and validation summary appears
        var error = _page.GetByTestId("cheep-error");
        await error.WaitForAsync();
        Assert.Contains("160", await error.InnerTextAsync());
    }

    [Fact(DisplayName = "Posting cheep persists and appears in timeline")]
    public async Task PostCheep_PersistsAndAppears()
    {
        await RegisterNewUserAsync();
        await _page.GotoAsync(BaseUrl + "/");
        var message = "E2E Test Cheep " + Guid.NewGuid();
        await _page.GetByTestId("cheep-input").FillAsync(message);
        await _page.GetByTestId("cheep-submit").ClickAsync();
        // Redirect reloads timeline; wait for message list containing our text
        await _page.WaitForSelectorAsync($"text={message}");
        Assert.True(await _page.GetByText(message).IsVisibleAsync());
    }

    private async Task RegisterNewUserAsync()
    {
        var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
        var email = $"test_{unique}@example.com";
        var password = "Passw0rd!"; // Must satisfy password policy

        await _page.GotoAsync(BaseUrl + "/Account/Register");
        await _page.FillAsync("input[name=\"Input.Name\"]", "TestUser" + unique);
        await _page.FillAsync("input[name=\"Input.Email\"]", email);
        await _page.FillAsync("input[name=\"Input.Password\"]", password);
        await _page.FillAsync("input[name=\"Input.ConfirmPassword\"]", password);
        await _page.ClickAsync("button:has-text('Register')");

        // After successful registration we should be redirected to home
        await _page.WaitForURLAsync(url => url.StartsWith(BaseUrl + "/"));
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