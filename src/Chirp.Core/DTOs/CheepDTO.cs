namespace Chirp.Core.DTOs;

// DTO used by Razor views. Only primitive/string fields.
public record CheepDTO(string Author, string Message, string Timestamp)
{
    public int AuthorId { get; init; }

    public int CheepId { get; init; }

    public int LikeCount { get; init; }

    public bool LikedByCurrentUser { get; init; }
}
