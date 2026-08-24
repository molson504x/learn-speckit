using Microsoft.AspNetCore.SignalR;

namespace Taskify.NotificationsApi.Hubs;

public sealed class BoardHub : Hub
{
    public Task JoinProject(string projectId)
        => this.Groups.AddToGroupAsync(this.Context.ConnectionId, projectId);

    public Task LeaveProject(string projectId)
        => this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, projectId);
}
