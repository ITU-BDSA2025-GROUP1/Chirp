using System.Collections.Generic;

namespace Chirp.Core.DTOs;

public record AuthorProfileDTO
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public int FollowerCount { get; init; }
    public int FollowingCount { get; init; }
    public IReadOnlyList<string> FollowingNames { get; init; } = Array.Empty<string>();
}
