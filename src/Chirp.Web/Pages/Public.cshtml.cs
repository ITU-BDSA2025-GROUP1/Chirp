using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    private readonly IAuthorRepository _authorRepository;

    private readonly ICheepRepository _cheepRepository;

    // keep a single property (DTOs) and initialize to avoid CS8618
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public int CurrentPage { get; set; }

    public string? CurrentAuthorName { get; set; }

    public PublicModel(ICheepService service, IAuthorRepository authorRepository, ICheepRepository cheepRepository)
    {
        _service = service;
        _authorRepository = authorRepository;
        _cheepRepository = cheepRepository;
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
    
    public ActionResult OnPost([FromForm] string text, [FromQuery] int page = 1)
    {
        // Always reload existing cheeps so we can re-render with validation errors
        LoadCheeps(page);

        if (User?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(User.Identity?.Name))
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

    private void LoadCheeps(int page)
    {
        if (page < 1) page = 1;
        CurrentPage = page;
        Cheeps = _service.GetCheeps(page);
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

    public async Task<IActionResult> OnPostLikeAsync(int cheepId)
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
            _cheepRepository.LikeCheep(cheepId, currentAuthor.Id);
        }
        catch (InvalidOperationException ex)
        {
            // Error handling to be done here
        }

        return RedirectToPage("/Public", new { page = CurrentPage });
    }
}


