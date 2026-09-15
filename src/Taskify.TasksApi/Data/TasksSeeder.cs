using Microsoft.EntityFrameworkCore;
using Taskify.ServiceDefaults.Models;
using Taskify.ServiceDefaults.ReferenceData;
using Taskify.ServiceDefaults.Validation;
using TaskEntity = Taskify.TasksApi.Models.Task;

namespace Taskify.TasksApi.Data;

public static class TasksSeeder
{
    public static Task SeedAsync(TasksDbContext dbContext, CancellationToken cancellationToken = default)
        => SeedAsyncCore(dbContext, cancellationToken);

    private static async Task SeedAsyncCore(TasksDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Tasks.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        List<TaskEntity> tasks =
        [
            CreateTask(
                Guid.Parse("10000000-0000-0000-0000-000000000001"),
                SampleProjects.Q4RoadmapId,
                "Define launch milestones",
                "Capture the milestones required to ship the Q4 plan.",
                PredefinedUsers.MayaChenId,
                KanbanColumn.ToDo,
                now),
            CreateTask(
                Guid.Parse("10000000-0000-0000-0000-000000000002"),
                SampleProjects.Q4RoadmapId,
                "Prepare stakeholder briefing",
                "Summarize trade-offs for the kickoff review.",
                PredefinedUsers.JordanLeeId,
                KanbanColumn.InProgress,
                now.AddMinutes(5),
                CreateComment(
                    Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    PredefinedUsers.PriyaShahId,
                    "Added the engineering risks section for tomorrow's review.",
                    now.AddMinutes(6))),
            CreateTask(
                Guid.Parse("10000000-0000-0000-0000-000000000003"),
                SampleProjects.BugTriageId,
                "Investigate payment retry bug",
                "Verify the repro steps from support before the triage session.",
                PredefinedUsers.LuisGarciaId,
                KanbanColumn.InReview,
                now.AddMinutes(10)),
            CreateTask(
                Guid.Parse("10000000-0000-0000-0000-000000000004"),
                SampleProjects.BugTriageId,
                "Close duplicate crash reports",
                null,
                null,
                KanbanColumn.Done,
                now.AddMinutes(15)),
            CreateTask(
                Guid.Parse("10000000-0000-0000-0000-000000000005"),
                SampleProjects.ResearchSprintId,
                "Compare onboarding analytics tools",
                "List the trade-offs between the current vendor options.",
                PredefinedUsers.AvaWilliamsId,
                KanbanColumn.ToDo,
                now.AddMinutes(20)),
        ];

        dbContext.Tasks.AddRange(tasks);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TaskEntity CreateTask(
        Guid id,
        Guid projectId,
        string title,
        string? description,
        Guid? assigneeUserId,
        KanbanColumn column,
        DateTime createdAtUtc,
        params Models.Comment[] comments)
        => new()
        {
            Id = id,
            ProjectId = projectId,
            Title = InputValidation.NormalizeRequiredText(title, nameof(title)),
            Description = InputValidation.NormalizeNullableText(description),
            AssigneeUserId = assigneeUserId is null ? null : InputValidation.RequirePredefinedUserId(assigneeUserId.Value, nameof(assigneeUserId)),
            Column = column,
            CreatedAtUtc = createdAtUtc,
            Comments = comments.ToList(),
        };

    private static Models.Comment CreateComment(Guid id, Guid authorUserId, string text, DateTime createdAtUtc)
        => new()
        {
            Id = id,
            AuthorUserId = InputValidation.RequirePredefinedUserId(authorUserId, nameof(authorUserId)),
            Text = InputValidation.NormalizeRequiredText(text, nameof(text)),
            CreatedAtUtc = createdAtUtc,
        };
}
