using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Taskify.ServiceDefaults.ReferenceData;
using Taskify.Web.Components;
using Taskify.Web.Services;

namespace Taskify.Web.Tests;

public sealed class DragDropTests : TestContext
{
    [Fact]
    public async Task HandleDrop_CallsTaskMoveWithDraggedTaskAndColumn()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        var module = JSInterop.SetupModule("./js/sortable-interop.js");
        module.SetupVoid("initializeKanbanBoard");

        var project = new ProjectSummary(SampleProjects.Q4RoadmapId, "Q4 Roadmap", DateTime.UtcNow);
        var task = new BoardTask(
            Guid.NewGuid(),
            project.Id,
            "Prepare stakeholder briefing",
            null,
            PredefinedUsers.JordanLeeId,
            "ToDo",
            DateTime.UtcNow,
            []);

        var tasksClient = new RecordingTasksApiClient(task);

        Services.AddLogging();
        Services.AddSingleton<ProjectsApiClient>(new FakeProjectsApiClient(project));
        Services.AddSingleton<TasksApiClient>(tasksClient);
        Services.AddSingleton<NotificationsApiClient>(new FakeNotificationsApiClient());
        Services.AddSingleton<DragDropService>(new DragDropService(JSInterop.JSRuntime));

        var cut = RenderComponent<KanbanBoard>(parameters => parameters
            .Add(component => component.ProjectId, project.Id)
            .Add(component => component.ActiveUserId, PredefinedUsers.MayaChenId));

        await cut.Instance.HandleDropAsync(task.Id.ToString(), "Done");

        Assert.Equal(task.Id, tasksClient.LastTaskId);
        Assert.Equal("Done", tasksClient.LastColumn);
        Assert.Equal(PredefinedUsers.MayaChenId, tasksClient.LastActingUserId);
    }

    private sealed class FakeProjectsApiClient(ProjectSummary project) : ProjectsApiClient(new HttpClient())
    {
        public override Task<ProjectSummary?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<ProjectSummary?>(project);
    }

    private sealed class RecordingTasksApiClient(BoardTask task) : TasksApiClient(new HttpClient())
    {
        public Guid LastTaskId { get; private set; }

        public string? LastColumn { get; private set; }

        public Guid LastActingUserId { get; private set; }

        public override Task<IReadOnlyList<BoardTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<BoardTask>>([task]);

        public override Task<BoardTask?> MoveTaskAsync(Guid taskId, string column, Guid actingUserId, CancellationToken cancellationToken)
        {
            LastTaskId = taskId;
            LastColumn = column;
            LastActingUserId = actingUserId;
            return Task.FromResult<BoardTask?>(task with { Column = column });
        }
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
