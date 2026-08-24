using Microsoft.EntityFrameworkCore;
using Taskify.ProjectsApi.Models;

namespace Taskify.ProjectsApi.Data;

public sealed class ProjectsDbContext(DbContextOptions<ProjectsDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => this.Set<Project>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ModelBuilderEntityProject(modelBuilder);
    }

    private static void ModelBuilderEntityProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(project => project.Id);
            entity.Property(project => project.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(project => project.Name).IsUnique();
        });
    }
}
