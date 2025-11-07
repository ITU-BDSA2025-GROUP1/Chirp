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
        return _db.Authors.FirstOrDefault(a => a.Name == name);

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

}