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
        [Display(Name = "Full name")]
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
        _logger.LogInformation("Posted Input.Email='{Email}' Request.Form[Input.Email]={FormVal}", Input?.Email, Request.Form["Input.Email"]);
        _logger.LogInformation("ModelState valid: {Valid}; Errors: {Errors}", ModelState.IsValid,
            string.Join(" | ", ModelState.Values.SelectMany(v=>v.Errors).Select(e=>e.ErrorMessage)));

        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new Author
        {
            UserName = Input.Email,
            Email = Input.Email,
            Name = Input.Name
        };

        _logger.LogWarning("Input.Email:'{Email}' Input.Name:'{Name}'", Input.Email, Input.Name);
        _logger.LogWarning("user.Email:'{Email}' user.UserName:'{UserName}'", user.Email, user.UserName);
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
            {
                _logger.LogWarning(user.Email);
                ModelState.AddModelError(string.Empty, err.Description);
                _logger.LogWarning("Register error: {Code} {Desc}", err.Code, err.Description);
            }
            return Page();
        }

        // sign in if you don't require confirmed account
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}
