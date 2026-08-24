using Microsoft.EntityFrameworkCore;
using Taskify.NotificationsApi.Models;

namespace Taskify.NotificationsApi.Data;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<NotificationEvent> NotificationEvents => this.Set<NotificationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationEvent>(entity =>
        {
            entity.ToTable("NotificationEvents");
            entity.HasKey(notificationEvent => notificationEvent.Id);
            entity.Property(notificationEvent => notificationEvent.EventType).IsRequired().HasMaxLength(100);
            entity.Property(notificationEvent => notificationEvent.Payload).IsRequired();
            entity.HasIndex(notificationEvent => new { notificationEvent.ProjectId, notificationEvent.OccurredAtUtc });
        });
    }
}
