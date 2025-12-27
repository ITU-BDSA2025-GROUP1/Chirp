using System.Collections.Generic;
using Chirp.Core.Entities;
using Xunit;
using Chirp.Web.Pages;
using Chirp.Core.Interfaces;
using Chirp.Web.Services;
using System.Security.Claims;
using System.Threading;
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
        var authorService = new FakeAuthorService();
        var model = new UserTimelineModel(service, authorService, new FakeForgetMeService());

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
        var authorService = new FakeAuthorService();
        var model = new UserTimelineModel(service, authorService, new FakeForgetMeService());

        model.OnGet("TestUser", 1);

        Assert.Equal(1, model.CurrentPage);
    }

    [Fact]
    public void UserTimeLineModel_ShouldPopulateProfile()
    {
        var service = new FakeCheepService();
        var authorService = new FakeAuthorService();
        var model = new UserTimelineModel(service, authorService, new FakeForgetMeService());

        model.OnGet("ProfileUser", 1);

        Assert.NotNull(model.Profile);
        Assert.Equal("ProfileUser", model.Profile!.Name);
        Assert.False(model.ViewingOwnProfile);
    }
}

// Fake implementation of ICheepService for testing purposes
public class FakeCheepService : ICheepService
{
    public List<CheepDTO> GetCheeps(int page = 1, int pageSize = 32, int? viewerId = null)
    {
        return new List<CheepDTO>();
    }

    public List<CheepDTO> GetCheepsFromAuthor(string author, int page = 1, int pageSize = 32, int? viewerId = null)
    {
        return new List<CheepDTO>
        {
            new CheepDTO("TestUser", "Test message", DateTime.UtcNow.ToString())
        };
    }

    public List<CheepDTO> GetCheepsFromAuthorAndFollowing(string author, int page = 1, int pageSize = 32, int? viewerId = null)
    {
        return new List<CheepDTO>
        {
            new CheepDTO("TestUser", "Test message from author and following", DateTime.UtcNow.ToString())
        };
    }

    public bool CreateCheep(string authorName, string text, DateTime? timestamp = null)
    {
        return true;
    }
}

public class FakeAuthorService : IAuthorService
{
    public AuthorDTO? GetAuthorByName(string name) => new AuthorDTO(1, "TestUser", "testuser@example.com");

    public AuthorDTO? GetAuthorByEmail(string email) => new AuthorDTO(1, "TestUser", email);

    public AuthorProfileDTO? GetProfileByName(string name) => new AuthorProfileDTO
    {
        Id = 2,
        Name = name,
        Email = $"{name}@example.com",
        FollowerCount = 3,
        FollowingCount = 2,
        FollowingNames = new List<string> { "Alice", "Bob" }
    };

    public void AddAuthor(Author author)
    {
        // No-op for tests
    }

    public void Follow(string followerName, string followeeName)
    {
        // No-op for tests
    }

    public void Unfollow(string followerName, string followeeName)
    {
        // No-op for tests
    }

    public bool IsFollowing(string followerName, string followeeName) => false;
}

public class FakeForgetMeService : IForgetMeService
{
    public ForgetMeResult Result { get; set; } = ForgetMeResult.Successful();

    public Task<ForgetMeResult> ForgetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result);
    }
}