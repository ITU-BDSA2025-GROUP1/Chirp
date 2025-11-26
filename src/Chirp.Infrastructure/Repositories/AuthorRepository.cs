using Chirp.Core.Entities;
using Chirp.Core.Interfaces;
using Chirp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Chirp.Infrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly ChirpDbContext _db;

    public AuthorRepository(ChirpDbContext db)
    {
        _db = db;
    }

    public Author? GetAuthorByName(string name)
    {
        return _db.Authors
        .Include(a => a.Following)
        .Include(a => a.Followers)
        .FirstOrDefault(a => a.Name == name);

    }

    public Author? GetAuthorByEmail(string email)
    {
        return _db.Authors.FirstOrDefault(a => a.Email == email);
    }

    public void AddAuthor(Author author)
    {
        if (_db.Authors.Any(a => a.Email == author.Email))
        {
            throw new InvalidOperationException("Email already used for a Chirp! account.");
        }
        else
        {
            _db.Authors.Add(author);
            _db.SaveChanges();
        }
    }

    public void Follow(string followerName, string followeeName)
    {
        var follower = GetAuthorByName(followerName);
        var followee = GetAuthorByName(followeeName);

        if (follower == null || followee == null)
        {
            throw new InvalidOperationException("Follower or followee does not exist.");
        }

        if (!follower.Following.Contains(followee))
        {
            follower.Following.Add(followee);
            _db.SaveChanges();
        }
    }

    public void Unfollow(string followerName, string followeeName)
    {
        var follower = GetAuthorByName(followerName);
        var followee = GetAuthorByName(followeeName);

        if (follower == null || followee == null)
        {
            throw new InvalidOperationException("Follower or followee does not exist.");
        }

        if (follower.Following.Contains(followee))
        {
            follower.Following.Remove(followee);
            _db.SaveChanges();
        }
    }

    public bool IsFollowing(string followerName, string followeeName)
    {
        var follower = GetAuthorByName(followerName);
        var followee = GetAuthorByName(followeeName);

        if (follower == null || followee == null)
        {
            throw new InvalidOperationException("Follower or followee does not exist.");
        }

        return follower.Following.Contains(followee);
    }
}