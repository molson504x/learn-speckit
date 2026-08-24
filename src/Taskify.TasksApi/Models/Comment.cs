namespace Taskify.TasksApi.Models;

public sealed class Comment
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public Guid AuthorUserId { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public Taskify.TasksApi.Models.Task? Task { get; set; }
}
