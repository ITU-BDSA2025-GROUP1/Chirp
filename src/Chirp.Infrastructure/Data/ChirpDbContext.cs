using Chirp.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Chirp.Infrastructure.Data;

public class ChirpDbContext : IdentityDbContext
{
    public ChirpDbContext(DbContextOptions<ChirpDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Cheep> Cheeps => Set<Cheep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Author>(b =>
        {
            b.HasKey(a => a.AuthorId);
            b.Property(a => a.Name).IsRequired().HasMaxLength(64);
            b.Property(a => a.Email).IsRequired().HasMaxLength(256);
            b.HasIndex(a => a.Name).IsUnique();
            b.HasIndex(a => a.Email).IsUnique();

            b.HasMany(a => a.Cheeps)
             .WithOne(c => c.Author)
             .HasForeignKey(c => c.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cheep>(b =>
        {
            b.HasKey(c => c.CheepId);
            b.Property(c => c.Text).IsRequired().HasMaxLength(280);

            // Ensure DateTime is tracked as UTC
            b.Property(c => c.Timestamp)
             .HasConversion(
                 v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                 v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            b.HasIndex(c => c.Timestamp);
        });
    }
}
