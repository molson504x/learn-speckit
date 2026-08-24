using Microsoft.AspNetCore.SignalR;

namespace Taskify.NotificationsApi.Hubs;

public sealed class BoardHub : Hub
{
    public Task JoinProject(string projectId)
    {
        if (!Guid.TryParse(projectId, out var parsedProjectId))
        {
            throw new HubException("Invalid projectId.");
        }

        return this.Groups.AddToGroupAsync(this.Context.ConnectionId, parsedProjectId.ToString("D"));
    }

    public Task LeaveProject(string projectId)
    {
        if (!Guid.TryParse(projectId, out var parsedProjectId))
        {
            throw new HubException("Invalid projectId.");
        }

        return this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, parsedProjectId.ToString("D"));
    }
}
