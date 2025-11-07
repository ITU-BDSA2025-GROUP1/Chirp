using Chirp.Core.DTOs;
using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);

    Boolean CreateCheep(Author author, string text, DateTime? timestamp = null);
}
