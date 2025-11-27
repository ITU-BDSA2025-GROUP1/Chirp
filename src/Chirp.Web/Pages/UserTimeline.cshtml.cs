using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IAuthorRepository _authorRepository;

    public List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage { get; set; }
    public string Author { get; set; }

    public string? CurrentAuthorName { get; set; }

    public UserTimelineModel(ICheepService service, IAuthorRepository authorRepository)
    {
        _service = service;
        _authorRepository = authorRepository;
    }

    public ActionResult OnGet(string author, [FromQuery] int page = 1)
    {
        CurrentPage = page;
        var email = User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var currentAuthor = _authorRepository.GetAuthorByEmail(email);
            CurrentAuthorName = currentAuthor?.Name;
        }

        if (page < 1) page = 1;
        
        Author = author;
        CurrentPage = page;
        Cheeps = _service.GetCheepsFromAuthor(author, page);
        return Page();
    }

    public ActionResult OnPost(string author, [FromForm] string text)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return RedirectToPage("/Public");
        }

        var currentUser = User?.Identity?.Name;
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (isAuthenticated && !string.IsNullOrWhiteSpace(currentUser) && !string.IsNullOrWhiteSpace(text))
        {
            _service.CreateCheep(currentUser!, text);
        }

        return RedirectToPage("/UserTimeline", new { author });
    }

    public bool IsFollowing(string followerName, string followeeName)
    {
        try
        {
            return _authorRepository.IsFollowing(followerName, followeeName);
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
        public async Task<IActionResult> OnPostFollowAsync(string authorName)
    {
          var email = User?.Identity?.Name;
        
        
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Login");
        }

        var currentAuthor = _authorRepository.GetAuthorByEmail(email);
        if (currentAuthor == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            _authorRepository.Follow(currentAuthor.Name, authorName);
        }
        catch (InvalidOperationException ex)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/UserTimeline", new { author = authorName, page = CurrentPage });
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string authorName)
    {
          var email = User?.Identity?.Name;
        
        
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToPage("/Login");
        }

        var currentAuthor = _authorRepository.GetAuthorByEmail(email);
        if (currentAuthor == null)
        {
            return RedirectToPage("/Login");
        }

        try
        {
            _authorRepository.Unfollow(currentAuthor.Name, authorName);
        }
        catch (InvalidOperationException ex)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/UserTimeline", new { author = authorName, page = CurrentPage });
    }
}
