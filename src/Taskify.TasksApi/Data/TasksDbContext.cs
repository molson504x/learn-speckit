using Microsoft.EntityFrameworkCore;
using Taskify.TasksApi.Models;
using TaskEntity = Taskify.TasksApi.Models.Task;

namespace Taskify.TasksApi.Data;

public sealed class TasksDbContext(DbContextOptions<TasksDbContext> options) : DbContext(options)
{
    public DbSet<TaskEntity> Tasks => this.Set<TaskEntity>();

    public DbSet<Comment> Comments => this.Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Title).IsRequired().HasMaxLength(200);
            entity.Property(task => task.Description).HasMaxLength(4_000);
            entity.Property(task => task.Column).HasConversion<int>();
            entity.HasMany(task => task.Comments)
                .WithOne(comment => comment.Task)
                .HasForeignKey(comment => comment.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(comment => comment.Id);
            entity.Property(comment => comment.Text).IsRequired().HasMaxLength(4_000);
            entity.HasIndex(comment => new { comment.TaskId, comment.CreatedAtUtc });
        });
    }
}
