using Chirp.Core.DTOs;
using Chirp.Core.Interfaces;
using System.Globalization;
using Chirp.Core.Entities;

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
        return author != null ? new AuthorDTO(author.Name) : null;
    }

    public AuthorDTO? GetAuthorByEmail(string email)
    {
        var author = _repo.GetAuthorByEmail(email);
        return author != null ? new AuthorDTO(author.Name) : null;
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