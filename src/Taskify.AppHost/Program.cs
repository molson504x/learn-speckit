using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var projectsApi = builder.AddProject("projectsapi", "../Taskify.ProjectsApi/Taskify.ProjectsApi.csproj")
    .WithEnvironment("ConnectionStrings__ProjectsDb", "Data Source=data/projects.db");

var tasksApi = builder.AddProject("tasksapi", "../Taskify.TasksApi/Taskify.TasksApi.csproj")
    .WithEnvironment("ConnectionStrings__TasksDb", "Data Source=data/tasks.db");

var notificationsApi = builder.AddProject("notificationsapi", "../Taskify.NotificationsApi/Taskify.NotificationsApi.csproj")
    .WithEnvironment("ConnectionStrings__NotificationsDb", "Data Source=data/notifications.db");

builder.AddProject("web", "../Taskify.Web/Taskify.Web.csproj")
    .WithReference(projectsApi)
    .WithReference(tasksApi)
    .WithReference(notificationsApi);

builder.Build().Run();
