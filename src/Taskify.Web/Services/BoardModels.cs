using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace Taskify.Web.Services;

public sealed record ProjectSummary(Guid Id, string Name, DateTime CreatedAtUtc);

public sealed record BoardTask(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssigneeUserId,
    string Column,
    DateTime CreatedAtUtc,
    IReadOnlyList<BoardComment> Comments);

public sealed record BoardComment(Guid Id, Guid AuthorUserId, string Text, DateTime CreatedAtUtc);

public sealed record TaskMovedEvent(
    Guid TaskId,
    Guid ProjectId,
    string FromColumn,
    string ToColumn,
    Guid ActingUserId,
    DateTime OccurredAtUtc);

public class ProjectsApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;

    public virtual async Task<IReadOnlyList<ProjectSummary>> GetProjectsAsync(CancellationToken cancellationToken)
        => await HttpClient.GetFromJsonAsync<List<ProjectSummary>>("/api/projects", cancellationToken) ?? [];

    public virtual Task<ProjectSummary?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken)
        => HttpClient.GetFromJsonAsync<ProjectSummary>($"/api/projects/{projectId}", cancellationToken);
}

public class TasksApiClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;

    public virtual async Task<IReadOnlyList<BoardTask>> GetTasksAsync(Guid projectId, CancellationToken cancellationToken)
        => await HttpClient.GetFromJsonAsync<List<BoardTask>>($"/api/tasks?projectId={projectId}", cancellationToken) ?? [];

    public virtual async Task<BoardTask?> MoveTaskAsync(Guid taskId, string column, Guid actingUserId, CancellationToken cancellationToken)
    {
        using var response = await HttpClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/column",
            new MoveTaskRequest(column, actingUserId),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BoardTask>(cancellationToken);
    }

    private sealed record MoveTaskRequest(string Column, Guid ActingUserId);
}

public class NotificationsApiClient(HttpClient httpClient, IHttpMessageHandlerFactory httpMessageHandlerFactory)
{
    private const string ConnectionName = "notifications-board";

    public HttpClient HttpClient { get; } = httpClient;

    public virtual async Task<IAsyncDisposable> SubscribeToBoardAsync(
        Guid projectId,
        Func<TaskMovedEvent, Task> onTaskMoved,
        CancellationToken cancellationToken)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(
                new Uri($"http://notificationsapi/hubs/board"),
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => httpMessageHandlerFactory.CreateHandler(ConnectionName);
                })
            .WithAutomaticReconnect()
            .Build();

        connection.On<TaskMovedEvent>("TaskMoved", onTaskMoved);
        await connection.StartAsync(cancellationToken);
        await connection.InvokeAsync("JoinProject", projectId.ToString(), cancellationToken);

        return new BoardSubscription(connection, projectId);
    }

    private sealed class BoardSubscription(HubConnection connection, Guid projectId) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            if (connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("LeaveProject", projectId.ToString());
            }

            await connection.DisposeAsync();
        }
    }
}
