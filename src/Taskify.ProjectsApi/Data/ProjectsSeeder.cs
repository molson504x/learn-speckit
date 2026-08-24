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
            CreateProject("Roadmap Planning", PredefinedUsers.MayaChenId, now),
            CreateProject("Platform Stabilization", PredefinedUsers.JordanLeeId, now.AddMinutes(1)),
            CreateProject("Customer Feedback Loop", PredefinedUsers.PriyaShahId, now.AddMinutes(2)));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Project CreateProject(string name, Guid createdByUserId, DateTime createdAtUtc)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = InputValidation.NormalizeRequiredText(name, nameof(name)),
            CreatedByUserId = InputValidation.RequirePredefinedUserId(createdByUserId, nameof(createdByUserId)),
            CreatedAtUtc = createdAtUtc,
        };
}
