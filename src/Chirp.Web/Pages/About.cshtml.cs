using Chirp.Core.DTOs;
using Chirp.Core.Entities;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Web.Pages;

[Authorize]

public class AboutModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly UserManager<Author> _userManager;

    public string DisplayName { get; private set; } = string.Empty; 
    public string Email { get; private set; } = string.Empty;
    public List<CheepDTO> Cheeps { get; private set; } = new(); 
    public int CheepCount { get; private set; } = 0;
    public int FollowingCount { get; private set; } = 0;
    public int FollowerCount { get; private set; } = 0;
    public UserStats UserInfo { get; private set; } = new();

    
    public AboutModel(ICheepService cheepService, UserManager<Author> userManager)
    {
        _cheepService = cheepService;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Account/Login");
        }

        var author = await _userManager.GetUserAsync(User);
        if (author == null)
        {
            return RedirectToPage("/Account/Login");
        }

        var username = author.Name ?? author.UserName ?? string.Empty;
        DisplayName = username;
        Email = author.Email ?? author.UserName ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(username))
        {
            Cheeps = _cheepService.GetCheepsFromAuthor(username, pageSize: 160);
        }

        UserInfo = new UserStats
        {
            FollowingCount = 0,
            FollowerCount = 0,
            CheepCount = Cheeps.Count
        };

        return Page();
    }

    public class UserStats
    {
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
        public int CheepCount { get; set; }
    }
}

