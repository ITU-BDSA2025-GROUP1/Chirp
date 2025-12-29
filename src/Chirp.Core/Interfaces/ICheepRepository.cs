using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface ICheepRepository
{
    List<Cheep> GetCheeps(int page = 1, int pageSize = 32);
    List<Cheep> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);

    List<Cheep> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32);

    Boolean CreateCheep(Cheep cheep);

    Boolean CreateCheep(string authorName, string text, DateTime? timestamp = null);

    Boolean LikeCheep (int cheepId, int authorId);

    Boolean IsLiked (int cheepId, int authorId);
}
