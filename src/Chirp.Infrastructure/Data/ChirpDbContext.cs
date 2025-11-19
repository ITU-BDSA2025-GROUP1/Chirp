using Chirp.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Infrastructure.Data;

public class ChirpDbContext : IdentityDbContext<Author, IdentityRole<int>, int>
{
    public ChirpDbContext(DbContextOptions<ChirpDbContext> options) : base(options) { }

    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Cheep> Cheeps => Set<Cheep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // required so Identity config (keys etc.) is applied

        modelBuilder.Entity<Author>(b =>
        {
            // no HasKey override for Identity primary key
            b.HasMany(a => a.Cheeps).WithOne(c => c.Author).HasForeignKey(c => c.AuthorId);
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
