using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface IAuthorRepository
{
    Author? GetAuthorByName(string name);

    Author? GetAuthorByEmail(string email);

    void AddAuthor(Author author);

    //Methods for follow/unfollow
    void Follow(string followerName, string followeeName);
    void Unfollow(string followerName, string followeeName);
    bool IsFollowing(string followerName, string followeeName);
}