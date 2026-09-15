using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Taskify.AppHost;
using Taskify.ServiceDefaults.ReferenceData;

namespace Taskify.AppHost.Tests;

public sealed class BoardRealTimeTests
{
    [Fact]
    public async Task MovingATask_BroadcastsTaskMovedToConnectedClients()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<AppHostMarker>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        using var tasksClient = app.CreateHttpClient("tasksapi");
        using var notificationsClient = app.CreateHttpClient("notificationsapi");
        var hubUrl = new Uri(notificationsClient.BaseAddress!, "/hubs/board");
        var receivedEvents = new ConcurrentQueue<TaskMovedContract>();
        var firstClientReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondClientReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var firstConnection = CreateConnection(hubUrl, receivedEvents, firstClientReceived);
        await using var secondConnection = CreateConnection(hubUrl, receivedEvents, secondClientReceived);

        await firstConnection.StartAsync();
        await secondConnection.StartAsync();
        await firstConnection.InvokeAsync("JoinProject", SampleProjects.Q4RoadmapId.ToString());
        await secondConnection.InvokeAsync("JoinProject", SampleProjects.Q4RoadmapId.ToString());

        using var response = await tasksClient.PatchAsJsonAsync(
            "/api/tasks/10000000-0000-0000-0000-000000000002/column",
            new MoveTaskRequest("Done", PredefinedUsers.MayaChenId));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await Task.WhenAll(firstClientReceived.Task, secondClientReceived.Task).WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(2, receivedEvents.Count);
        Assert.All(receivedEvents, taskMovedEvent =>
        {
            Assert.Equal(Guid.Parse("10000000-0000-0000-0000-000000000002"), taskMovedEvent.TaskId);
            Assert.Equal(SampleProjects.Q4RoadmapId, taskMovedEvent.ProjectId);
            Assert.Equal("Done", taskMovedEvent.ToColumn);
        });
    }

    private static HubConnection CreateConnection(
        Uri hubUrl,
        ConcurrentQueue<TaskMovedContract> receivedEvents,
        TaskCompletionSource receivedSignal)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        connection.On<TaskMovedContract>("TaskMoved", taskMovedEvent =>
        {
            receivedEvents.Enqueue(taskMovedEvent);
            receivedSignal.TrySetResult();
        });

        return connection;
    }

    private sealed record MoveTaskRequest(string Column, Guid ActingUserId);

    private sealed record TaskMovedContract(
        Guid TaskId,
        Guid ProjectId,
        string FromColumn,
        string ToColumn,
        Guid ActingUserId,
        DateTime OccurredAtUtc);
}
