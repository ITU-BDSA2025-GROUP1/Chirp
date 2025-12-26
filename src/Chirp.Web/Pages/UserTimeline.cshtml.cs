using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

[Authorize]
public class UserTimelineModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly IAuthorService _authorService;

    public List<CheepDTO> Cheeps { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public string Author { get; private set; } = string.Empty;
    public AuthorProfileDTO? Profile { get; private set; }
    public string? CurrentAuthorName { get; private set; }
    public int? CurrentAuthorId { get; private set; }
    public bool? IsFollowingProfile { get; private set; }

    public bool ViewingOwnProfile =>
        Profile != null &&
        CurrentAuthorName != null &&
        string.Equals(Profile.Name, CurrentAuthorName, StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> FollowingNames =>
        Profile?.FollowingNames ?? Array.Empty<string>();

    public UserTimelineModel(ICheepService cheepService, IAuthorService authorService)
    {
        _cheepService = cheepService;
        _authorService = authorService;
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
