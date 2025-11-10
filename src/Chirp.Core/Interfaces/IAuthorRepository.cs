using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface IAuthorRepository
{
    Author? GetAuthorByName(string name);

    Author? GetAuthorByEmail(string email);

    void AddAuthor(Author author);
}