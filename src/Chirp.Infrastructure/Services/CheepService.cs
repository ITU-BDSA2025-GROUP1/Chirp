using Chirp.Core.DTOs;
using Chirp.Core.Entities;
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
                FormatTs(c.Timestamp)))
            .ToList();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        var items = _repo.GetCheepsFromAuthor(author, page, pageSize);
        return items.Select(c => new CheepDTO(
                c.Author.Name,
                c.Text,
                FormatTs(c.Timestamp)))
            .ToList();
    }

    public Boolean CreateCheep(Author author, string text, DateTime? timestamp = null)
    {
        //if (text.Length > 160) return false;

        /*
            Change Author author in signature to be string author and lookup the author in the author table
            var authorEntity = _authorRepo.GetAuthorByName(author);
            And change Author = authorEntity in the Cheep object below
        */

        //var cheep = new Core.Entities.Cheep
        //{
        //Text = text,
        //Timestamp = timestamp ?? DateTime.UtcNow,
        //Author = author //Change to Author = authorEntity
        //};

        if (string.IsNullOrWhiteSpace(author.Name) || string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Length > 280)
            return false;

        return _repo.CreateCheep(author.Name, text, timestamp);
    }

    private static string FormatTs(DateTime dtUtc)
        => dtUtc.ToUniversalTime().ToString("HH:mm:ss dd MMM yyyy", CultureInfo.InvariantCulture);  // MMM = Oct, MMMM = October, yyyy = 2025, yy = 25
}
