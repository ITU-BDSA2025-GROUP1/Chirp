using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using System.Globalization;

namespace Chirp.Infrastructure.Services;

public class CheepService : ICheepService
{
    private readonly ICheepRepository _repo;

    public CheepService(ICheepRepository repo)
    {
        _repo = repo;
    }

    public List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32, int? viewerId = null)
    {
        var items = _repo.GetCheeps(page, pageSize) ?? Enumerable.Empty<Chirp.Core.Entities.Cheep>();
        return items.Select(c => 
        {
            var likes = c.Likes ?? new List<Chirp.Core.Entities.Author>();
            
            return new CheepDTO(
                c.Author?.Name ?? string.Empty,
                c.Text,
                FormatTs(c.Timestamp))
                {
                    AuthorId = c.AuthorId,
                    CheepId = c.CheepId,
                    LikeCount = likes.Count,
                    LikedByCurrentUser = viewerId.HasValue && likes.Any(a => a.Id == viewerId.Value)
                };
    }).ToList();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32, int? viewerId = null)
    {
        var items = _repo.GetCheepsFromAuthor(author, page, pageSize) ?? Enumerable.Empty<Chirp.Core.Entities.Cheep>();
        return items.Select(c => 
        {
            var likes = c.Likes ?? new List<Chirp.Core.Entities.Author>();
            
            return new CheepDTO(
                c.Author?.Name ?? string.Empty,
                c.Text,
                FormatTs(c.Timestamp))
                {
                    AuthorId = c.AuthorId,
                    CheepId = c.CheepId,
                    LikeCount = likes.Count,
                    LikedByCurrentUser = viewerId.HasValue && likes.Any(a => a.Id == viewerId.Value)
                };
    }).ToList();
    }

    public List<CheepDTO> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32, int? viewerId = null)
    {
        var items = _repo.GetCheepsFromAuthorAndFollowing(author, page, pageSize);
        return items.Select(c => 
        {
            var likes = c.Likes ?? new List<Chirp.Core.Entities.Author>();
            
            return new CheepDTO(
                c.Author?.Name ?? string.Empty,
                c.Text,
                FormatTs(c.Timestamp))
                {
                    AuthorId = c.AuthorId,
                    CheepId = c.CheepId,
                    LikeCount = likes.Count,
                    LikedByCurrentUser = viewerId.HasValue && likes.Any(a => a.Id == viewerId.Value)
                };
    }).ToList();
    }

    public bool CreateCheep(string authorName, string text, DateTime? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(authorName)) return false;
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.Length > 280) return false;

        return _repo.CreateCheep(authorName, text, timestamp);
    }
    private static string FormatTs(DateTime dtUtc)
        => dtUtc.ToUniversalTime().ToString("HH:mm:ss dd MMM yyyy", CultureInfo.InvariantCulture);  // MMM = Oct, MMMM = October, yyyy = 2025, yy = 25
}

