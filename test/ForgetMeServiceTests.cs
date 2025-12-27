using System;
using System.Collections.Generic;
using System.Security.Claims;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Data;
using Chirp.Web.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Chirp.Tests;

public class ForgetMeServiceTests
{
    [Fact]
    public async Task ForgetCurrentUserAsync_MasksSensitiveIdentityData()
    {
        using var context = BuildContext();
        using var userManager = BuildUserManager(context);
        var service = new ForgetMeService(userManager, context, NullLogger<ForgetMeService>.Instance);

        var author = new Author
        {
            Name = "Integration User",
            Email = "integration@example.com",
            UserName = "integration@example.com"
        };

        var createResult = await userManager.CreateAsync(author, "Passw0rd!");
        createResult.Succeeded.Should().BeTrue("test setup must create the author");

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, author.Id.ToString()),
            new Claim(ClaimTypes.Name, author.Email!)
        }, IdentityConstants.ApplicationScheme));

        var result = await service.ForgetCurrentUserAsync(principal);

        result.Success.Should().BeTrue();

        var updated = await userManager.FindByIdAsync(author.Id.ToString());
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Deleted User");
        updated.Email.Should().BeNull();
        updated.UserName.Should().StartWith("deleted-user-");
        updated.Followers.Should().BeEmpty();
        updated.Following.Should().BeEmpty();
    }

    private static ChirpDbContext BuildContext()
    {
        var options = new DbContextOptionsBuilder<ChirpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ChirpDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static UserManager<Author> BuildUserManager(ChirpDbContext context)
    {
        var store = new UserStore<Author, IdentityRole<int>, ChirpDbContext, int>(context);
        var options = Options.Create(new IdentityOptions());
        var passwordHasher = new PasswordHasher<Author>();
        var userValidators = new List<IUserValidator<Author>> { new UserValidator<Author>() };
        var passwordValidators = new List<IPasswordValidator<Author>> { new PasswordValidator<Author>() };
        var normalizer = new UpperInvariantLookupNormalizer();
        var describer = new IdentityErrorDescriber();
        var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        var logger = services.GetRequiredService<ILogger<UserManager<Author>>>();

        return new UserManager<Author>(
            store,
            options,
            passwordHasher,
            userValidators,
            passwordValidators,
            normalizer,
            describer,
            services,
            logger);
    }
}
