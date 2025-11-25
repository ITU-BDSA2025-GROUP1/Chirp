using System.Linq;
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
    private const int PageSize = 32;
    private readonly ICheepService _cheepService;
    private readonly UserManager<Author> _userManager;

    public string DisplayName { get; private set; } = string.Empty; 
    public string Email { get; private set; } = string.Empty;
    public List<CheepDTO> Cheeps { get; private set; } = new(); 
    public int CheepCount { get; private set; } = 0;
    public int FollowingCount { get; private set; } = 0;
    public int FollowerCount { get; private set; } = 0;
    public UserStats UserInfo { get; private set; } = new();
    public int CurrentPage { get; private set; } = 1;
    public bool HasNextPage { get; private set; }
    public bool HasPreviousPage => CurrentPage > 1;

    
    public AboutModel(ICheepService cheepService, UserManager<Author> userManager)
    {
        _cheepService = cheepService;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] int page = 1)
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

        var username = author.Name ?? string.Empty;
        DisplayName = username;
        Email = author.Email ?? author.UserName ?? string.Empty;
        if (page < 1) page = 1;
        CurrentPage = page;

        UserInfo = new UserStats
        {
            FollowingCount = 0,
            FollowerCount = 0,
            CheepCount = _cheepService.CountCheep(username)
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            Console.WriteLine("This is the input page: " + page);
            Cheeps = _cheepService.GetCheepsFromAuthor(username, page: CurrentPage, pageSize: PageSize);
            if (UserInfo.CheepCount > page * PageSize) HasNextPage = true;
        }
        else
        {
            HasNextPage = false;
            Cheeps = new List<CheepDTO>();
        }

        return Page();
    }

    public class UserStats
    {
        public int FollowingCount { get; set; }
        public int FollowerCount { get; set; }
        public int CheepCount { get; set; }
    }
}

