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
    public DbSet<AccountReadModel> Accounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserReadModel>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsDeleted);

            entity.HasOne(e => e.Account)
                .WithOne(e => e.User)
                .HasForeignKey<AccountReadModel>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AccountReadModel>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone).IsUnique();
        });
    }
}
