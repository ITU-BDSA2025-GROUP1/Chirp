using Chirp.Core.Entities;
using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface IAuthorService
{
    AuthorDTO? GetAuthorByName(string name);

    AuthorDTO? GetAuthorByEmail(string email);

    void AddAuthor(Author author);

    //Methods for follow/unfollow
    void Follow(string followerName, string followeeName);
    void Unfollow(string followerName, string followeeName);
    bool IsFollowing(string followerName, string followeeName);
}