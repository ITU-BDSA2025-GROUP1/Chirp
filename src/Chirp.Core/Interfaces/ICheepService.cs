using Chirp.Core.DTOs;
using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);

    List<CheepDTO> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32);

    Boolean CreateCheep(string authorName, string text, DateTime? timestamp = null);
}
