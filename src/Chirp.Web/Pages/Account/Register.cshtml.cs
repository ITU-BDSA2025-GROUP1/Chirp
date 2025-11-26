// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Pages.Account;
public class RegisterModel : PageModel
{
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(
        UserManager<Author> userManager,
        SignInManager<Author> signInManager,
        IEmailSender emailSender,
        ILogger<RegisterModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        _logger.LogInformation("Posted Input.Email='{Email}' Request.Form[Input.Email]={FormVal}",
            Input?.Email ?? string.Empty,
            Request.Form["Input.Email"].ToString());

        _logger.LogInformation("ModelState valid: {Valid}; Errors: {Errors}", ModelState.IsValid,
            string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

        returnUrl ??= Url.Content("~/");

        // guard: ensure model-bound Input isn't null (removes CS8602 warning)
        if (Input is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid form submission.");
            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new Author
        {
            UserName = Input.Email ?? string.Empty,
            Email = Input.Email ?? string.Empty,
            Name = Input.Name ?? string.Empty
        };

        var result = await _userManager.CreateAsync(user, Input?.Password ?? string.Empty);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError(string.Empty, err.Description);
            }
            return Page();
        }

        // sign in if you don't require confirmed account
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}
