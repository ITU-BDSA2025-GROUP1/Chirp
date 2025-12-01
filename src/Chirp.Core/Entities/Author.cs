using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Entities;


public class Author : IdentityUser<int>
{
    public int AuthorId { get; set; }

    public string Name { get; set; } = string.Empty;
    public override string? Email { get; set; }
    public override string? UserName { get; set; }

    public List<Cheep> Cheeps { get; set; } = new();

    //Authors this user follows
    public List<Author> Following { get; set; } = new();

    //Authors that follow this user
    public List<Author> Followers { get; set; } = new();
}
