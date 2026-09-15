using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Taskify.ServiceDefaults.Models;
using Taskify.ServiceDefaults.ReferenceData;
using Taskify.ServiceDefaults.Validation;
using Taskify.TasksApi.Data;
using TaskEntity = Taskify.TasksApi.Models.Task;

namespace Taskify.TasksApi.Endpoints;

public static class TasksEndpoints
{
    private const string NotificationsHttpClientName = "notificationsapi";
    private static readonly IReadOnlyDictionary<string, string[]> InvalidUserError = new Dictionary<string, string[]>
    {
        ["actingUserId"] = ["The selected user must be one of the predefined users."],
    };

    public static IEndpointRouteBuilder MapTasksEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/tasks");

        group.MapGet("/", GetTasksAsync);
        group.MapPatch("/{id:guid}/column", UpdateTaskColumnAsync);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<TaskResponse>>> GetTasksAsync(
        Guid projectId,
        TasksDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var tasks = await dbContext.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId)
            .OrderBy(task => task.Column)
            .ThenBy(task => task.CreatedAtUtc)
            .Select(task => new TaskResponse(
                task.Id,
                task.ProjectId,
                task.Title,
                task.Description,
                task.AssigneeUserId,
                task.Column.ToString(),
                task.CreatedAtUtc,
                task.Comments
                    .OrderBy(comment => comment.CreatedAtUtc)
                    .Select(comment => new CommentResponse(
                        comment.Id,
                        comment.AuthorUserId,
                        comment.Text,
                        comment.CreatedAtUtc))
                    .ToList()))
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<IReadOnlyList<TaskResponse>>(tasks);
    }

    private static async Task<Results<Ok<TaskResponse>, NotFound, BadRequest<ValidationErrorResponse>>> UpdateTaskColumnAsync(
        Guid id,
        UpdateTaskColumnRequest request,
        TasksDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (!PredefinedUsers.IsPredefinedUser(request.ActingUserId))
        {
            return TypedResults.BadRequest(new ValidationErrorResponse(InvalidUserError));
        }

        if (!TryParseColumn(request.Column, out var targetColumn))
        {
            return TypedResults.BadRequest(new ValidationErrorResponse(new Dictionary<string, string[]>
            {
                ["column"] = ["Column must be one of ToDo, InProgress, InReview, or Done."],
            }));
        }

        var task = await dbContext.Tasks
            .Include(candidate => candidate.Comments.OrderBy(comment => comment.CreatedAtUtc))
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (task is null)
        {
            return TypedResults.NotFound();
        }

        var originalColumn = task.Column;
        task.Column = targetColumn;
        await dbContext.SaveChangesAsync(cancellationToken);

        _ = SendTaskMovedNotificationAsync(
            httpClientFactory,
            loggerFactory.CreateLogger("TaskMovedNotifier"),
            task,
            originalColumn,
            request.ActingUserId);

        return TypedResults.Ok(MapTask(task));
    }

    private static bool TryParseColumn(string? value, out KanbanColumn column)
        => Enum.TryParse(value, ignoreCase: true, out column) && Enum.IsDefined(column);

    private static TaskResponse MapTask(TaskEntity task)
        => new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssigneeUserId,
            task.Column.ToString(),
            task.CreatedAtUtc,
            task.Comments
                .OrderBy(comment => comment.CreatedAtUtc)
                .Select(comment => new CommentResponse(
                    comment.Id,
                    comment.AuthorUserId,
                    comment.Text,
                    comment.CreatedAtUtc))
                .ToList());

    private static async Task SendTaskMovedNotificationAsync(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        TaskEntity task,
        KanbanColumn originalColumn,
        Guid actingUserId)
    {
        try
        {
            var client = httpClientFactory.CreateClient(NotificationsHttpClientName);
            var payload = new TaskMovedPayload(
                task.Id,
                task.ProjectId,
                originalColumn.ToString(),
                task.Column.ToString(),
                actingUserId,
                DateTime.UtcNow);

            using var response = await client.PostAsJsonAsync(
                "/api/notifications/events",
                new NotificationEventRequest(task.ProjectId, "TaskMoved", payload),
                CancellationToken.None);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Notifications API returned status code {StatusCode} while publishing task move for task {TaskId}.",
                    response.StatusCode,
                    task.Id);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to publish task move notification for task {TaskId}.", task.Id);
        }
    }

    internal sealed record UpdateTaskColumnRequest(string? Column, Guid ActingUserId);

    internal sealed record TaskResponse(
        Guid Id,
        Guid ProjectId,
        string Title,
        string? Description,
        Guid? AssigneeUserId,
        string Column,
        DateTime CreatedAtUtc,
        IReadOnlyList<CommentResponse> Comments);

    internal sealed record CommentResponse(Guid Id, Guid AuthorUserId, string Text, DateTime CreatedAtUtc);

    internal sealed record ValidationErrorResponse(IReadOnlyDictionary<string, string[]> Errors);

    internal sealed record NotificationEventRequest(Guid ProjectId, string EventType, TaskMovedPayload Payload);

    internal sealed record TaskMovedPayload(
        Guid TaskId,
        Guid ProjectId,
        string FromColumn,
        string ToColumn,
        Guid ActingUserId,
        DateTime OccurredAtUtc);
}
