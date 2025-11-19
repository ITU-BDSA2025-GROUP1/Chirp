#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.Entities;

namespace Chirp.Web.Pages.Account;

[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<Author> _signInManager;
    private readonly UserManager<Author> _userManager;
    private readonly ILogger<ExternalLoginModel> _logger;

    public ExternalLoginModel(
        SignInManager<Author> signInManager,
        UserManager<Author> userManager,
        ILogger<ExternalLoginModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ProviderDisplayName { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Display Name")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public IActionResult OnGet() => RedirectToPage("./Login");

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }
        
        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }
        else
        {
            // If the user does not have an account, then ask the user to create an account.
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
            
            // Log all claims for debugging
            _logger.LogInformation("Claims from {Provider}:", info.LoginProvider);
            foreach (var claim in info.Principal.Claims)
            {
                _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }
            
            // Get email from external login provider
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            }
            
            // If no email found, generate one based on the provider key
            if (string.IsNullOrEmpty(Input.Email))
            {
                // Use GitHub username or provider key to create a unique email
                var login = info.Principal.FindFirstValue(ClaimTypes.Name) 
                    ?? info.Principal.FindFirstValue("login")
                    ?? info.ProviderKey;
                Input.Email = $"{login}@github.oauth.local";
                _logger.LogWarning("No email provided by GitHub. Using generated email: {Email}", Input.Email);
            }
            
            // Try to get name from external login provider
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                Input.Name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            }
            else if (info.Principal.HasClaim(c => c.Type == "name"))
            {
                Input.Name = info.Principal.FindFirstValue("name") ?? string.Empty;
            }
            
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            // Check if user with this email already exists
            var existingUser = await _userManager.FindByEmailAsync(Input.Email);
            
            if (existingUser != null)
            {
                // User exists with this email, add the external login to their account
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(existingUser, isPersistent: false);
                    _logger.LogInformation("User {Email} added external login and signed in.", Input.Email);
                    return LocalRedirect(returnUrl);
                }
                else
                {
                    foreach (var error in addLoginResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }
            else
            {
                // Create a new user
                var user = new Author
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Name = Input.Name
                };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
        ReturnUrl = returnUrl;
        return Page();
    }
}
