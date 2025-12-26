using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using Chirp.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Chirp.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _repo;

    public AuthorService(IAuthorRepository repo)
    {
        _repo = repo;
    }

    public AuthorDTO? GetAuthorByName(string name)
    {
        var author = _repo.GetAuthorByName(name);
        return author != null ? new AuthorDTO(author.Id, author.Name, author.Email) : null;
    }

    public AuthorDTO? GetAuthorByEmail(string email)
    {
        var author = _repo.GetAuthorByEmail(email);
        return author != null ? new AuthorDTO(author.Id, author.Name, author.Email) : null;
    }

    public AuthorProfileDTO? GetProfileByName(string name)
    {
        var author = _repo.GetAuthorByName(name);
        if (author == null)
        {
            return null;
        }

        var followingNames = author.Following?
            .Where(a => !string.IsNullOrWhiteSpace(a.Name))
            .Select(a => a.Name)
            .OrderBy(n => n)
            .ToList()
            ?? new List<string>();

        return new AuthorProfileDTO
        {
            Id = author.Id,
            Name = author.Name,
            Email = author.Email,
            FollowerCount = author.Followers?.Count ?? 0,
            FollowingCount = author.Following?.Count ?? 0,
            FollowingNames = followingNames
        };
    }

    public void AddAuthor(Author author)
    {
        _repo.AddAuthor(author);
    }

    public void Follow(string followerName, string followeeName)
    {
        _repo.Follow(followerName, followeeName);
    }

    public void Unfollow(string followerName, string followeeName)
    {
        _repo.Unfollow(followerName, followeeName);
    }

    public bool IsFollowing(string followerName, string followeeName)
    {
        return _repo.IsFollowing(followerName, followeeName);
    }
}