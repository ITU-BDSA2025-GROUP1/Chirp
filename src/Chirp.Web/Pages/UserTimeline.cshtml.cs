using System;
using System.Threading;
using System.Threading.Tasks;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Chirp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Chirp.Web.Pages;

[Authorize]
public class UserTimelineModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;
    private readonly IForgetMeService _forgetMeService;

    public List<CheepDTO> Cheeps { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public string Author { get; private set; } = string.Empty;
    public AuthorProfileDTO? Profile { get; private set; }
    public string? CurrentAuthorName { get; private set; }
    public int? CurrentAuthorId { get; private set; }
    public bool? IsFollowingProfile { get; private set; }

    [TempData]
    public string? ForgetMeError { get; set; }

    public bool ViewingOwnProfile =>
        Profile != null &&
        CurrentAuthorName != null &&
        string.Equals(Profile.Name, CurrentAuthorName, StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> FollowingNames =>
        Profile?.FollowingNames ?? Array.Empty<string>();

    public UserTimelineModel(ICheepService cheepService, IAuthorService authorService, IForgetMeService forgetMeService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
        _forgetMeService = forgetMeService;
    }

    public ActionResult OnGet(string author, [FromQuery] int page = 1)
    {
        var viewer = ResolveCurrentAuthor();
        if (page < 1)
        {
            page = 1;
        }

        CurrentPage = page;
        IsFollowingProfile = null;
        Profile = _authorService.GetProfileByName(author);
        if (Profile == null)
        {
            return NotFound();
        }

        Author = Profile.Name;

        if (ViewingOwnProfile)
        {
            Cheeps = _cheepService.GetCheepsFromAuthorAndFollowing(Profile.Name, page, viewerId: viewer?.Id);
        }
        else
        {
            Cheeps = _cheepService.GetCheepsFromAuthor(Profile.Name, page, viewerId: viewer?.Id);
            if (viewer != null)
            {
                IsFollowingProfile = _authorService.IsFollowing(viewer.Name, Profile.Name);
            }
        }

        return Page();
    }

    public ActionResult OnPost(string author, [FromForm] string text, [FromQuery] int page = 1)
    {
        var viewer = ResolveCurrentAuthor();
        if (viewer == null)
        {
            return RedirectToPage("/Account/Login");
        }

        if (string.IsNullOrWhiteSpace(author))
        {
            return RedirectToPage("/Public");
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            _cheepService.CreateCheep(User.Identity!.Name!, text);
        }

        return RedirectToPage("/UserTimeline", new { author, page });
    }

    public IActionResult OnPostFollow(string profileName, int page)
    {
        var viewer = ResolveCurrentAuthor();
        if (viewer == null)
        {
            return RedirectToPage("/Account/Login");
        }

        _authorService.Follow(viewer.Name, profileName);
        return RedirectToPage("/UserTimeline", new { author = profileName, page });
    }

    public IActionResult OnPostUnfollow(string profileName, int page)
    {
        var viewer = ResolveCurrentAuthor();
        if (viewer == null)
        {
            return RedirectToPage("/Account/Login");
        }

        _authorService.Unfollow(viewer.Name, profileName);
        return RedirectToPage("/UserTimeline", new { author = profileName, page });
    }

    public async Task<IActionResult> OnPostForgetMeAsync(int page, CancellationToken cancellationToken)
    {
        CurrentPage = page < 1 ? 1 : page;
        var viewer = ResolveCurrentAuthor();
        if (viewer == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _forgetMeService.ForgetCurrentUserAsync(User, cancellationToken);
        if (!result.Success)
        {
            ForgetMeError = result.ErrorMessage ?? "We could not delete your profile.";
            return RedirectToPage("/UserTimeline", new { author = viewer.Name, page = CurrentPage });
        }

        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return RedirectToPage("/Public", new { forget = "1" });
    }

    private AuthorDTO? ResolveCurrentAuthor()
    {
        var email = User?.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            CurrentAuthorName = null;
            CurrentAuthorId = null;
            return null;
        }

        var viewer = _authorService.GetAuthorByEmail(email);
        CurrentAuthorName = viewer?.Name;
        CurrentAuthorId = viewer?.Id;
        return viewer;
    }
}
