using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IAuthorRepository _authorRepository;
    public List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage { get; set; }

    public string? CurrentAuthorName { get; set; }

    public PublicModel(ICheepService service, IAuthorRepository authorRepository)
    {
        _service = service;
        _authorRepository = authorRepository;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        CurrentPage = page;
        var email = User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var currentAuthor = _authorRepository.GetAuthorByEmail(email);
            CurrentAuthorName = currentAuthor?.Name;
        }

        if (page < 1) page = 1;

        CurrentPage = page;
        Cheeps = _service.GetCheeps(page);
        return Page();
    }
    
    public ActionResult OnPost([FromForm] string text)
    {
        if (User?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(User.Identity?.Name))
        {
            return RedirectToPage("/Public");
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            _service.CreateCheep(User.Identity!.Name!, text);
        }

        return RedirectToPage("/Public");
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

        return RedirectToPage("/Public", new { author = authorName, page = CurrentPage });
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

        return RedirectToPage("/Public", new { author = authorName, page = CurrentPage });
    }
}


