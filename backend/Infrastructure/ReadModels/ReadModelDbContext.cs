using FocusedBytes.Api.Infrastructure.ReadModels.Entities;
using Microsoft.EntityFrameworkCore;

namespace FocusedBytes.Api.Infrastructure.ReadModels;

public class ReadModelDbContext : DbContext
{
    public ReadModelDbContext(DbContextOptions<ReadModelDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserReadModel> Users { get; set; } = null!;
    public DbSet<AuthMethodReadModel> AuthMethods { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadModel>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.IsDeleted);

            entity.HasMany(e => e.AuthMethods)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthMethodReadModel>(entity =>
        {
            entity.ToTable("AuthMethods");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);

            // Composite unique index: same identifier+type combination cannot exist for the same user
            entity.HasIndex(e => new { e.Identifier, e.Type }).IsUnique();
        });
    }
}
