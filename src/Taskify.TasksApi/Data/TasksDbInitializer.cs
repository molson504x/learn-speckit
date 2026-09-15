using Microsoft.EntityFrameworkCore;

namespace Taskify.TasksApi.Data;

public static class TasksDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();

        if (dbContext.Database.GetMigrations().Any())
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

        await TasksSeeder.SeedAsync(dbContext, cancellationToken);
    }
}
