using Chirp.Core.DTOs;
using Chirp.Core.Entities;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Repositories;

public class CheepRepository : ICheepRepository
{
    private readonly ChirpDbContext _db;

    public CheepRepository(ChirpDbContext db)
    {
        _db = db;
    }

    public List<Cheep> GetCheeps(int page = 1, int pageSize = 32)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Cheep> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        if (string.IsNullOrWhiteSpace(author)) return new();
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        Console.WriteLine("This is the username: " + author);
        Console.WriteLine("This is the page: " + page);

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.Author != null && c.Author.Name == author)
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Cheep> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32)
    {
        if (string.IsNullOrWhiteSpace(author)) return new();
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 32;

        var timelineOwner = _db.Authors
            .Include(a => a.Following)
            .SingleOrDefault(a => a.Name == author);

        if (timelineOwner == null) return new();

        var authorIds = timelineOwner.Following.Select(a => a.Id).Append(timelineOwner.Id).ToList();

        return _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => authorIds.Contains(c.AuthorId))
            .OrderByDescending(c => c.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Boolean CreateCheep(Cheep cheep)
    {
        _db.Cheeps.Add(cheep);
        return _db.SaveChanges() > 0;
    }

    public bool CreateCheep(string authorName, string text, DateTime? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(authorName) || string.IsNullOrWhiteSpace(text))
            return false;

        if (text.Length > 280)
            return false;

        var author = _db.Authors.SingleOrDefault(a => a.Name == authorName);
        if (author == null)
        {
            author = _db.Authors.SingleOrDefault(a => a.Email == authorName);
        }
        if (author == null)
        {
            author = _db.Authors.SingleOrDefault(a => a.UserName == authorName);
        }
        if (author == null)
            return false;

        var cheep = new Cheep
        {
            Text = text.Trim(),
            Timestamp = timestamp ?? DateTime.UtcNow,
            AuthorId = author.Id
        };

        _db.Cheeps.Add(cheep);
        return _db.SaveChanges() > 0;

    }

    public int CountCheep(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName)) {
            return 0;
        }

        List<Cheep> cheeps = _db.Cheeps
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.Author.Name == authorName)
            .ToList();

        return cheeps.Count;
    }

}

