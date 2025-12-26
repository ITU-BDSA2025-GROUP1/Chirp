using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IAuthorService _authorService;

    private readonly ICheepRepository _cheepRepository;

    // keep a single property (DTOs) and initialize to avoid CS8618
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public int CurrentPage { get; set; }

    public string? CurrentAuthorName { get; set; }
    public int? CurrentAuthorId { get; set; }

    public PublicModel(ICheepService service, IAuthorService authorService, ICheepRepository cheepRepository)
    {
        _service = service;
        _authorService = authorService;
        _cheepRepository = cheepRepository;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        var currentAuthor = ResolveCurrentAuthor();
        if (page < 1) page = 1;

        CurrentPage = page;
        Cheeps = _service.GetCheeps(page, viewerId: currentAuthor?.Id);
        return Page();
    }
    
    public ActionResult OnPost([FromForm] string text, [FromQuery] int page = 1)
    {
        // Always reload existing cheeps so we can re-render with validation errors
        var currentAuthor = ResolveCurrentAuthor();
        LoadCheeps(page, currentAuthor?.Id);

        if (currentAuthor == null)
        {
            // Not authenticated: do nothing other than show reminder
            ModelState.AddModelError("text", "You must be logged in to post a cheep.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            ModelState.AddModelError("text", "Cheep cannot be empty.");
            return Page();
        }

        if (text.Length > 160)
        {
            ModelState.AddModelError("text", "Cheep exceeds 160 characters.");
            return Page();
        }

        _service.CreateCheep(User.Identity!.Name!, text);
        return RedirectToPage("/Public", new { page });
    }

    private void LoadCheeps(int page, int? viewerId)
    {
        if (page < 1) page = 1;
        CurrentPage = page;
        Cheeps = _service.GetCheeps(page, viewerId: viewerId);
    }


    public bool IsFollowing(string? followerName, string followeeName)
    {
        if (string.IsNullOrWhiteSpace(followerName) || string.IsNullOrWhiteSpace(followeeName))
        {
            return false;
        }
        try
        {
            return _authorService.IsFollowing(followerName, followeeName);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
    public IActionResult OnPostFollow(string authorName)
    {
        var currentAuthor = ResolveCurrentAuthor();
        if (currentAuthor == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            _authorService.Follow(currentAuthor.Name, authorName);
        }
        catch (InvalidOperationException)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/Public", new { author = authorName, page = CurrentPage });
    }

    public IActionResult OnPostUnfollow(string authorName)
    {
        var currentAuthor = ResolveCurrentAuthor();
        if (currentAuthor == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            _authorService.Unfollow(currentAuthor.Name, authorName);
        }
        catch (InvalidOperationException)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/Public", new { author = authorName, page = CurrentPage });
    }

    public IActionResult OnPostLike(int cheepId)
    {
        var currentAuthor = ResolveCurrentAuthor();
        if (currentAuthor == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            _cheepRepository.LikeCheep(cheepId, currentAuthor.Id);
        }
        catch (InvalidOperationException)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/Public", new { page = CurrentPage });
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

        var currentAuthor = _authorService.GetAuthorByEmail(email);
        CurrentAuthorName = currentAuthor?.Name;
        CurrentAuthorId = currentAuthor?.Id;
        return currentAuthor;
    }
}


