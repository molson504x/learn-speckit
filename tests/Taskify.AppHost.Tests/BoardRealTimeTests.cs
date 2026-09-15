extern alias NotificationsApi;
extern alias TasksApi;

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Taskify.ServiceDefaults.ReferenceData;

namespace Taskify.AppHost.Tests;

public sealed class BoardRealTimeTests
{
    [Fact]
    public async Task MovingATask_BroadcastsTaskMovedToConnectedClients()
    {
        await using var notificationsFactory = new NotificationsApiFactory();
        using var notificationsClient = notificationsFactory.CreateClient();
        await using var tasksFactory = new TasksApiFactory(notificationsClient.BaseAddress!, notificationsFactory);
        using var tasksClient = tasksFactory.CreateClient();
        var hubUrl = new Uri(notificationsClient.BaseAddress!, "/hubs/board");
        var receivedEvents = new ConcurrentQueue<TaskMovedContract>();
        var firstClientReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondClientReceived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var firstConnection = CreateConnection(notificationsFactory, hubUrl, receivedEvents, firstClientReceived);
        await using var secondConnection = CreateConnection(notificationsFactory, hubUrl, receivedEvents, secondClientReceived);

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
        NotificationsApiFactory notificationsFactory,
        Uri hubUrl,
        ConcurrentQueue<TaskMovedContract> receivedEvents,
        TaskCompletionSource receivedSignal)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(
                hubUrl,
                options =>
                {
                    options.HttpMessageHandlerFactory = _ => notificationsFactory.Server.CreateHandler();
                })
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

    private sealed class NotificationsApiFactory : WebApplicationFactory<NotificationsApi::Program>
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"taskify-notifications-{Guid.NewGuid():N}.db");

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:NotificationsDb"] = $"Data Source={databasePath}",
                });
            });
        }
    }

    private sealed class TasksApiFactory(Uri notificationsBaseAddress, NotificationsApiFactory notificationsFactory)
        : WebApplicationFactory<TasksApi::Program>
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"taskify-integration-tasks-{Guid.NewGuid():N}.db");

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }

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

            builder.ConfigureServices(services =>
            {
                services.AddHttpClient("notificationsapi", client =>
                {
                    client.BaseAddress = notificationsBaseAddress;
                })
                .ConfigurePrimaryHttpMessageHandler(() => notificationsFactory.Server.CreateHandler());
            });
        }
    }
}
