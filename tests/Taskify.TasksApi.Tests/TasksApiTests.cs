using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Taskify.ServiceDefaults.ReferenceData;

namespace Taskify.TasksApi.Tests;

public sealed class TasksApiTests
{
    [Fact]
    public async Task GetTasks_ReturnsProjectTasksWithCommentsSorted()
    {
        await using var factory = new TasksApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/tasks?projectId={SampleProjects.Q4RoadmapId}");
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskContract>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(tasks);
        Assert.NotEmpty(tasks);
        Assert.All(tasks, task => Assert.Equal(SampleProjects.Q4RoadmapId, task.ProjectId));

        var taskWithComment = Assert.Single(tasks.Where(task => task.Comments.Count > 0));
        Assert.True(taskWithComment.Comments.SequenceEqual(taskWithComment.Comments.OrderBy(comment => comment.CreatedAtUtc)));
    }

    [Fact]
    public async Task PatchTaskColumn_MovesTaskAndPreservesOtherFields()
    {
        await using var factory = new TasksApiFactory();
        using var client = factory.CreateClient();

        var originalTask = (await client.GetFromJsonAsync<List<TaskContract>>($"/api/tasks?projectId={SampleProjects.Q4RoadmapId}"))!
            .Single(task => task.Comments.Count > 0);

        using var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{originalTask.Id}/column",
            new MoveTaskRequest("Done", PredefinedUsers.MayaChenId));

        var updatedTask = await response.Content.ReadFromJsonAsync<TaskContract>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedTask);
        Assert.Equal("Done", updatedTask.Column);
        Assert.Equal(originalTask.Title, updatedTask.Title);
        Assert.Equal(originalTask.Description, updatedTask.Description);
        Assert.Equal(originalTask.AssigneeUserId, updatedTask.AssigneeUserId);
        Assert.Equal(originalTask.ProjectId, updatedTask.ProjectId);
        Assert.Equal(originalTask.Comments, updatedTask.Comments);
    }

    [Fact]
    public async Task PatchTaskColumn_RejectsInvalidColumn()
    {
        await using var factory = new TasksApiFactory();
        using var client = factory.CreateClient();

        var originalTask = (await client.GetFromJsonAsync<List<TaskContract>>($"/api/tasks?projectId={SampleProjects.Q4RoadmapId}"))!
            .First();

        using var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{originalTask.Id}/column",
            new MoveTaskRequest("Blocked", PredefinedUsers.MayaChenId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record TaskContract(
        Guid Id,
        Guid ProjectId,
        string Title,
        string? Description,
        Guid? AssigneeUserId,
        string Column,
        DateTime CreatedAtUtc,
        IReadOnlyList<CommentContract> Comments);

    private sealed record CommentContract(Guid Id, Guid AuthorUserId, string Text, DateTime CreatedAtUtc);

    private sealed record MoveTaskRequest(string Column, Guid ActingUserId);

    private sealed class TasksApiFactory : WebApplicationFactory<global::Program>
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"taskify-tasks-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TasksDb"] = $"Data Source={databasePath}",
                });
            });
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
