using Chirp.Core.DTOs;
using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface ICheepService
{
    List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32, int? viewerId = null);
    List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32, int? viewerId = null);

    List<CheepDTO> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32, int? viewerId = null);

    Boolean CreateCheep(string authorName, string text, DateTime? timestamp = null);
}
