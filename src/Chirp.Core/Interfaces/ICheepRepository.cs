using Chirp.Core.Entities;

namespace Chirp.Core.Interfaces;

public interface ICheepRepository
{
    List<Cheep> GetCheeps(int page = 1, int pageSize = 32);
    List<Cheep> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32);

    Boolean CreateCheep(Cheep cheep);
}
