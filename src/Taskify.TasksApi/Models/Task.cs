using Taskify.ServiceDefaults.Models;

namespace Taskify.TasksApi.Models;

public sealed class Task
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? AssigneeUserId { get; set; }

    public KanbanColumn Column { get; set; } = KanbanColumn.ToDo;

    public DateTime CreatedAtUtc { get; set; }

    public List<Comment> Comments { get; set; } = [];
}
