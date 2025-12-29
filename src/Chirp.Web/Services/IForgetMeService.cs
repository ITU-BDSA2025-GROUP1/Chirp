using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Chirp.Web.Services;

public interface IForgetMeService
{
    Task<ForgetMeResult> ForgetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
}

public record ForgetMeResult(bool Success, string? ErrorMessage)
{
    public static ForgetMeResult Successful() => new(true, null);
    public static ForgetMeResult Failed(string message) => new(false, message);
}
