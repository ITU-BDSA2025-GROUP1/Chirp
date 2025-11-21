using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;

namespace Chirp.Web.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;

    // keep a single property (DTOs) and initialize to avoid CS8618
    public List<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    public int CurrentPage { get; set; }

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
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
}