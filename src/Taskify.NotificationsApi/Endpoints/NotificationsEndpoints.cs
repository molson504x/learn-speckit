using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Taskify.NotificationsApi.Data;
using Taskify.NotificationsApi.Hubs;
using Taskify.NotificationsApi.Models;

namespace Taskify.NotificationsApi.Endpoints;

public static class NotificationsEndpoints
{
    private static readonly HashSet<string> SupportedEventTypes =
    [
        BoardHub.TaskMovedMethod,
        BoardHub.TaskCreatedMethod,
        BoardHub.TaskAssignedMethod,
        BoardHub.CommentAddedMethod,
    ];

    public static IEndpointRouteBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications");

        group.MapPost("/events", CreateEventAsync);
        group.MapGet("/events", GetEventsAsync);

        return endpoints;
    }

    private static async Task<IResult> CreateEventAsync(
        CreateNotificationEventRequest request,
        NotificationsDbContext dbContext,
        IHubContext<BoardHub> hubContext,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var notificationEvent = new NotificationEvent
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            EventType = NormalizeEventType(request.EventType),
            Payload = request.Payload.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : request.Payload.GetRawText(),
            OccurredAtUtc = DateTime.UtcNow,
        };

        ArgumentException.ThrowIfNullOrWhiteSpace(notificationEvent.EventType);
        ArgumentOutOfRangeException.ThrowIfEqual(notificationEvent.ProjectId, Guid.Empty);

        dbContext.NotificationEvents.Add(notificationEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await BoardHub.BroadcastAsync(
                hubContext,
                notificationEvent.ProjectId,
                notificationEvent.EventType,
                ParsePayload(notificationEvent.Payload),
                cancellationToken);
        }
        catch (Exception exception)
        {
            loggerFactory.CreateLogger("NotificationBroadcaster")
                .LogWarning(exception, "Failed to broadcast notification event {EventType} for project {ProjectId}.", notificationEvent.EventType, notificationEvent.ProjectId);
        }

        return Results.Accepted();
    }

    private static async Task<Ok<IReadOnlyList<NotificationEventResponse>>> GetEventsAsync(
        Guid projectId,
        DateTime? since,
        NotificationsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var query = dbContext.NotificationEvents
            .AsNoTracking()
            .Where(notificationEvent => notificationEvent.ProjectId == projectId);

        if (since is not null)
        {
            query = query.Where(notificationEvent => notificationEvent.OccurredAtUtc >= since.Value);
        }

        var events = await query
            .OrderBy(notificationEvent => notificationEvent.OccurredAtUtc)
            .Select(notificationEvent => new NotificationEventResponse(
                notificationEvent.Id,
                notificationEvent.ProjectId,
                notificationEvent.EventType,
                ParsePayload(notificationEvent.Payload),
                notificationEvent.OccurredAtUtc))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyList<NotificationEventResponse>>(events);
    }

    private static JsonElement ParsePayload(string payload)
        => JsonDocument.Parse(string.IsNullOrWhiteSpace(payload) ? "{}" : payload).RootElement.Clone();

    private static string NormalizeEventType(string? value)
    {
        var normalizedValue = value?.Trim() ?? string.Empty;
        if (!SupportedEventTypes.Contains(normalizedValue))
        {
            throw new ArgumentException("Unsupported event type.", nameof(value));
        }

        return normalizedValue;
    }

    internal sealed record CreateNotificationEventRequest(Guid ProjectId, string? EventType, JsonElement Payload);

    internal sealed record NotificationEventResponse(
        Guid Id,
        Guid ProjectId,
        string EventType,
        JsonElement Payload,
        DateTime OccurredAtUtc);
}
