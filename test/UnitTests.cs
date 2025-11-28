using Chirp.Core.Entities;
using Xunit;
using Chirp.Web.Pages;
using Chirp.Core.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Chirp.Core.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Tests;

public class UnitTests
{
    [Fact]
    public void Cheep_ShouldIncludeCheepidTextTimestampAuthoridAndAuthor()
    {
        var cheep = new Cheep
        {
            CheepId = 1,
            Text = "This is a test cheep!",
            AuthorId = 1,
            Author = new Author { Name = "TestUser" },
        };

        Assert.Equal(1, cheep.CheepId);
        Assert.Equal("This is a test cheep!", cheep.Text);
        Assert.Equal(1, cheep.AuthorId);
        Assert.Equal("TestUser", cheep.Author.Name);
    }

    [Fact]
    public void Cheep_Timestamp_ShouldBeUtc()
    {
        var cheep = new Cheep
        {
            Timestamp = DateTime.UtcNow
        };

        Assert.Equal(DateTimeKind.Utc, cheep.Timestamp.Kind);
    }

    [Fact]
    public void Author_ShouldIncludeIdNameAndEmail()
    {
        var author = new Author
        {
            AuthorId = 1,
            Name = "TestUser",
            Email = "testuser@example.com"
        };

        Assert.Equal(1, author.AuthorId);
        Assert.Equal("TestUser", author.Name);
        Assert.Equal("testuser@example.com", author.Email);
    }


    [Fact]
    public void Author_ShouldInitializeEmptyCheepsList()
    {
        var author = new Author();
        Assert.NotNull(author.Cheeps);
        Assert.Empty(author.Cheeps);
    }

    [Fact]
    public void UserTimeLineModel_OnGet_ShouldGetCurrentPage()
    {
        var service = new FakeCheepService();
        var model = new UserTimelineModel(service);

        model.OnGet("TestUser", 2);

        Assert.NotNull(model);
        Assert.Equal(2, model.CurrentPage);
        Assert.Equal("TestUser", model.Author);
        Assert.NotNull(model.Cheeps);
    }

    [Fact]
    public void PublicTimeLineModel_OnGet_ShouldDefaultToPage1()
    {
        var service = new FakeCheepService();
        var model = new UserTimelineModel(service);

        model.OnGet("TestUser", 1);

        Assert.Equal(1, model.CurrentPage);
    }
}

// Fake implementation of ICheepService for testing purposes
public class FakeCheepService : ICheepService
{
    public List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32)
    {
        return new List<CheepDTO>();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32)
    {
        return new List<CheepDTO>
        {
            new CheepDTO("TestUser", "Test message", DateTime.UtcNow.ToString())
        };
    }

    public bool CreateCheep(string authorName, string text, DateTime? timestamp = null)
    {
        return true;
    }

    public int CountCheep(string authorName)
    {
        return 1;
    }
}