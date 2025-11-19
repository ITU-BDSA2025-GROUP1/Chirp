#nullable enable
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Chirp.Core.Entities;

namespace Chirp.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<Author> _signInManager;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(SignInManager<Author> signInManager, ILogger<LogoutModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Logout page GET request - User: {User}", User.Identity?.Name ?? "Anonymous");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        _logger.LogInformation("Logout POST request - User: {User}", User.Identity?.Name ?? "Anonymous");
        
        // SignInManager.SignOutAsync handles everything for Identity
        await _signInManager.SignOutAsync();
        
        _logger.LogInformation("User logged out successfully.");
        
        // Add cache control headers to prevent caching
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";
        
        returnUrl = returnUrl ?? Url.Content("~/");
        
        // Use Redirect instead of LocalRedirect to force a full page reload
        return Redirect(returnUrl);
    }
}