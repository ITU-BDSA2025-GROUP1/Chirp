using Chirp.Core.Entities;
using Chirp.Core.DTOs;

namespace Chirp.Core.Interfaces;

public interface IAuthorService
{
    AuthorDTO? GetAuthorByName(string name);

    AuthorDTO? GetAuthorByEmail(string email);

    void AddAuthor(Author author);
}