using Aspire.Hosting;
using System.IO;

var builder = DistributedApplication.CreateBuilder(args);
var repositoryRoot = ResolveRepositoryRoot();

var projectsApi = builder.AddProject("projectsapi", Path.Combine(repositoryRoot, "src/Taskify.ProjectsApi/Taskify.ProjectsApi.csproj"))
    .WithEnvironment("ConnectionStrings__ProjectsDb", "Data Source=data/projects.db");

var tasksApi = builder.AddProject("tasksapi", Path.Combine(repositoryRoot, "src/Taskify.TasksApi/Taskify.TasksApi.csproj"))
    .WithEnvironment("ConnectionStrings__TasksDb", "Data Source=data/tasks.db");

var notificationsApi = builder.AddProject("notificationsapi", Path.Combine(repositoryRoot, "src/Taskify.NotificationsApi/Taskify.NotificationsApi.csproj"))
    .WithEnvironment("ConnectionStrings__NotificationsDb", "Data Source=data/notifications.db");

builder.AddProject("web", Path.Combine(repositoryRoot, "src/Taskify.Web/Taskify.Web.csproj"))
    .WithReference(projectsApi)
    .WithReference(tasksApi)
    .WithReference(notificationsApi);

builder.Build().Run();

static string ResolveRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Taskify.slnx")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Could not locate the repository root containing Taskify.slnx.");
}

public partial class Program;
