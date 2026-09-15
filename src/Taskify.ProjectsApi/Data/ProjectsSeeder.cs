using Microsoft.EntityFrameworkCore;
using Taskify.ProjectsApi.Models;
using Taskify.ServiceDefaults.ReferenceData;
using Taskify.ServiceDefaults.Validation;

namespace Taskify.ProjectsApi.Data;

public static class ProjectsSeeder
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        if (dbContext.Database.GetMigrations().Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        if (await dbContext.Projects.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        dbContext.Projects.AddRange(
            CreateProject(SampleProjects.All[0].Id, SampleProjects.All[0].Name, SampleProjects.All[0].CreatedByUserId, now),
            CreateProject(SampleProjects.All[1].Id, SampleProjects.All[1].Name, SampleProjects.All[1].CreatedByUserId, now.AddMinutes(1)),
            CreateProject(SampleProjects.All[2].Id, SampleProjects.All[2].Name, SampleProjects.All[2].CreatedByUserId, now.AddMinutes(2)));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Project CreateProject(Guid id, string name, Guid createdByUserId, DateTime createdAtUtc)
        => new()
        {
            Id = id,
            Name = InputValidation.NormalizeRequiredText(name, nameof(name)),
            CreatedByUserId = InputValidation.RequirePredefinedUserId(createdByUserId, nameof(createdByUserId)),
            CreatedAtUtc = createdAtUtc,
        };
}
