using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Entities;


public class Author : IdentityUser<int>
{
    public int AuthorId { get; set; }

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;

    public List<Cheep> Cheeps { get; set; } = new();
}
