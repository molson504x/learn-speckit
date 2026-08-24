namespace Taskify.NotificationsApi.Models;

public sealed class NotificationEvent
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Payload { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; }
}
