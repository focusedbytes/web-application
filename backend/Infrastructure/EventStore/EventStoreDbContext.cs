using Microsoft.EntityFrameworkCore;

namespace FocusedBytes.Api.Infrastructure.EventStore;

public class EventStoreDbContext : DbContext
{
    public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<EventStoreEntity> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EventStoreEntity>(entity =>
        {
            entity.ToTable("EventStore");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AggregateId);
            entity.HasIndex(e => new { e.AggregateId, e.Version }).IsUnique();
            entity.Property(e => e.EventData).HasColumnType("jsonb");
        });
    }
}
