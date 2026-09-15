using Microsoft.AspNetCore.SignalR;

namespace Taskify.NotificationsApi.Hubs;

public sealed class BoardHub : Hub
{
    public const string TaskMovedMethod = "TaskMoved";
    public const string TaskCreatedMethod = "TaskCreated";
    public const string TaskAssignedMethod = "TaskAssigned";
    public const string CommentAddedMethod = "CommentAdded";

    public Task JoinProject(string projectId)
        => this.Groups.AddToGroupAsync(this.Context.ConnectionId, projectId);

    public Task LeaveProject(string projectId)
        => this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, projectId);

    public static Task BroadcastAsync(
        IHubContext<BoardHub> hubContext,
        Guid projectId,
        string methodName,
        object payload,
        CancellationToken cancellationToken = default)
        => hubContext.Clients.Group(projectId.ToString()).SendAsync(methodName, payload, cancellationToken);
}
