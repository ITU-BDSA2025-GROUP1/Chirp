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

    public List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32)
    {
        var items = _repo.GetCheeps(page, pageSize);
        return items.Select(c => new CheepDTO(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp))
                {
                    AuthorId = c.AuthorId
                })
            .ToList();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        var items = _repo.GetCheepsFromAuthor(author, page, pageSize);
        return items.Select(c => new CheepDTO(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp))
                {
                    AuthorId = c.AuthorId
                })
            .ToList();
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

