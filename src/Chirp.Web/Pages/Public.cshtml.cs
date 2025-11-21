using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepDTO> Cheeps { get; set; }
    public int CurrentPage { get; set; }

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        LoadCheeps(page);
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
}


