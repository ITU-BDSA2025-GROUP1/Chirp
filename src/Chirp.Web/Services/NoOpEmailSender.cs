using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;

namespace Chirp.Web.Services;

public sealed class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;
    public NoOpEmailSender(ILogger<NoOpEmailSender> logger) => _logger = logger;

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Email suppressed (dev): to={Email}, subject={Subject}, len={Len}",
            email, subject, htmlMessage?.Length ?? 0);
        return Task.CompletedTask;
    }
}