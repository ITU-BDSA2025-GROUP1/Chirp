#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Chirp.Core.Entities;

namespace Chirp.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<Author> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<Author> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (!ModelState.IsValid) return Page();

        // You set UserName = Email at registration, so sign in with email as username
        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in: {Email}", Input.Email);
            return LocalRedirect(ReturnUrl!);
        }

        ModelState.AddModelError(string.Empty, "There’s no user with that email or password. Please check your email and password."); // Changed from: "Invalid login attempt."
        return Page();
    }

    public IActionResult OnPostGitHub(string? returnUrl = null)
    {
        // Store the return URL for after authentication
        var redirectUrl = returnUrl ?? Url.Content("~/");
        
        // Configure the redirect URL and which provider to use
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl = redirectUrl }));
        
        // This triggers the challenge (redirect to GitHub)
        return new ChallengeResult("GitHub", properties);
    }
}