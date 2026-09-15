using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Taskify.ServiceDefaults.ReferenceData;
using Taskify.Web.Components;
using Taskify.Web.Services;

namespace Taskify.Web.Tests;

public sealed class KanbanBoardTests : TestContext
{
    [Fact]
    public void KanbanBoard_RendersFourColumnsAndTasks()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule("./js/sortable-interop.js");
        module.SetupVoid("initializeKanbanBoard");

        var project = new ProjectSummary(SampleProjects.Q4RoadmapId, "Q4 Roadmap", DateTime.UtcNow);
        var tasks = new[]
        {
            CreateTask(Guid.NewGuid(), project.Id, "Define launch milestones", "ToDo", 0),
            CreateTask(Guid.NewGuid(), project.Id, "Prepare stakeholder briefing", "InProgress", 1),
            CreateTask(Guid.NewGuid(), project.Id, "Review dependency risks", "InReview", 2),
            CreateTask(Guid.NewGuid(), project.Id, "Share launch recap", "Done", 3),
        };

        Services.AddLogging();
        Services.AddSingleton<ProjectsApiClient>(new FakeProjectsApiClient(project));
        Services.AddSingleton<TasksApiClient>(new FakeTasksApiClient(tasks));
        Services.AddSingleton<NotificationsApiClient>(new FakeNotificationsApiClient());
        Services.AddSingleton<DragDropService>(new DragDropService(JSInterop.JSRuntime));

        var cut = RenderComponent<KanbanBoard>(parameters => parameters
            .Add(component => component.ProjectId, project.Id)
            .Add(component => component.ActiveUserId, PredefinedUsers.MayaChenId));

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(4, cut.FindAll("[data-column]").Count);
            Assert.Contains("To Do", cut.Markup);
            Assert.Contains("In Progress", cut.Markup);
            Assert.Contains("In Review", cut.Markup);
            Assert.Contains("Done", cut.Markup);
            Assert.Contains("Define launch milestones", cut.Markup);
            Assert.Contains("Prepare stakeholder briefing", cut.Markup);
        });
    }

    [Fact]
    public void KanbanBoard_RendersAllColumnsForEmptyProject()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule("./js/sortable-interop.js");
        module.SetupVoid("initializeKanbanBoard");

        var project = new ProjectSummary(Guid.NewGuid(), "Empty Project", DateTime.UtcNow);

        Services.AddLogging();
        Services.AddSingleton<ProjectsApiClient>(new FakeProjectsApiClient(project));
        Services.AddSingleton<TasksApiClient>(new FakeTasksApiClient([]));
        Services.AddSingleton<NotificationsApiClient>(new FakeNotificationsApiClient());
        Services.AddSingleton<DragDropService>(new DragDropService(JSInterop.JSRuntime));

        var cut = RenderComponent<KanbanBoard>(parameters => parameters
            .Add(component => component.ProjectId, project.Id)
            .Add(component => component.ActiveUserId, PredefinedUsers.MayaChenId));

        cut.WaitForAssertion(() =>
        {
            Assert.Equal(4, cut.FindAll("[data-column]").Count);
            Assert.Equal(4, cut.FindAll(".kanban-empty").Count);
            Assert.DoesNotContain("alert-danger", cut.Markup);
        });
    }

    private static BoardTask CreateTask(Guid taskId, Guid projectId, string title, string column, int minutes)
        => new(
            taskId,
            projectId,
            title,
            $"{title} description",
            PredefinedUsers.JordanLeeId,
            column,
            DateTime.UtcNow.AddMinutes(minutes),
            []);

    private sealed class FakeProjectsApiClient(ProjectSummary project) : ProjectsApiClient(new HttpClient())
    {
        public override Task<ProjectSummary?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<ProjectSummary?>(project);
    }

    private sealed class FakeTasksApiClient(IEnumerable<BoardTask> tasks) : TasksApiClient(new HttpClient())
    {
        private readonly IReadOnlyList<BoardTask> taskItems = tasks.ToList();

        public override Task<IReadOnlyList<BoardTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult(taskItems);
    }

    private sealed class FakeNotificationsApiClient() : NotificationsApiClient(new HttpClient(), new FakeHandlerFactory())
    {
        public override Task<IAsyncDisposable> SubscribeToBoardAsync(
            Guid projectId,
            Func<TaskMovedEvent, Task> onTaskMoved,
            CancellationToken cancellationToken)
            => Task.FromResult<IAsyncDisposable>(new NoopAsyncDisposable());
    }

    private sealed class FakeHandlerFactory : IHttpMessageHandlerFactory
    {
        public HttpMessageHandler CreateHandler(string name) => new HttpClientHandler();
    }

    private sealed class NoopAsyncDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
