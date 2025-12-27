using System.Security.Claims;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Services;

public sealed class ForgetMeService : IForgetMeService
{
    private static readonly string PlaceholderName = "Deleted User";

    private readonly UserManager<Author> _userManager;
    private readonly ChirpDbContext _db;
    private readonly ILogger<ForgetMeService> _logger;

    public ForgetMeService(UserManager<Author> userManager, ChirpDbContext db, ILogger<ForgetMeService> logger)
    {
        _userManager = userManager;
        _db = db;
        _logger = logger;
    }

    public async Task<ForgetMeResult> ForgetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return ForgetMeResult.Failed("You must be signed in to perform this action.");
        }

        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return ForgetMeResult.Failed("We were unable to find your profile.");
        }

        await LoadNavigationCollectionsAsync(user, cancellationToken);

        user.Followers.Clear();
        user.Following.Clear();
        user.LikedCheeps.Clear();

        user.Name = PlaceholderName;
        var placeholderEmail = BuildPlaceholderEmail(user.Id);
        user.Email = placeholderEmail;
        user.NormalizedEmail = placeholderEmail.ToUpperInvariant();
        user.PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.EmailConfirmed = false;
        user.AccessFailedCount = 0;
        user.LockoutEnabled = false;
        user.LockoutEnd = null;
        user.TwoFactorEnabled = false;

        var placeholderUserName = BuildPlaceholderUserName(user.Id);
        user.UserName = placeholderUserName;
        user.NormalizedUserName = placeholderUserName.ToUpperInvariant();

        user.PasswordHash = null;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.ConcurrencyStamp = Guid.NewGuid().ToString("N");

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to anonymize user {UserId}: {Errors}", user.Id, errors);
            return ForgetMeResult.Failed("We couldn't anonymize your profile right now. Please try again.");
        }

        var logins = await _userManager.GetLoginsAsync(user);
        foreach (var login in logins)
        {
            await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        }

        _db.UserTokens.RemoveRange(_db.UserTokens.Where(t => t.UserId == user.Id));
        _db.UserClaims.RemoveRange(_db.UserClaims.Where(c => c.UserId == user.Id));
        await _db.SaveChangesAsync(cancellationToken);

        return ForgetMeResult.Successful();
    }

    private static string BuildPlaceholderUserName(int userId)
    {
        var value = $"deleted-user-{userId}-{Guid.NewGuid():N}";
        return value.Length <= 256 ? value : value[..256];
    }

    private static string BuildPlaceholderEmail(int userId)
        => $"deleted-user-{userId}-{Guid.NewGuid():N}@chirp.local";

    private async Task LoadNavigationCollectionsAsync(Author user, CancellationToken cancellationToken)
    {
        await _db.Entry(user).Collection(a => a.Followers).LoadAsync(cancellationToken);
        await _db.Entry(user).Collection(a => a.Following).LoadAsync(cancellationToken);
        await _db.Entry(user).Collection(a => a.LikedCheeps).LoadAsync(cancellationToken);
    }
}
