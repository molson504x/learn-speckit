# Quickstart: Validate Taskify Kanban Workspace

## Prerequisites

- .NET 9 SDK with Aspire workload: `dotnet workload install aspire`
- Docker Desktop (or Podman) running, if the AppHost is configured to containerize the SQLite volumes/services
- No database server install needed — each service manages its own SQLite file

## Run the app

```bash
cd src/Taskify.AppHost
dotnet run
```

- Opens the Aspire dashboard, which lists `projectsapi`, `tasksapi`, `notificationsapi`, and `web` resources with their endpoints, logs, and traces.
- On first run, each API applies its EF Core migrations and seeds data: `Taskify.ProjectsApi` seeds the 3 sample projects (FR-003); `Taskify.ServiceDefaults` seeds the 5 predefined users (FR-001) consumed by Projects/Tasks/Web.
- Open the `web` resource's endpoint from the dashboard to reach the Blazor Server frontend.

## Validate User Story 1 — Track work on a project board (P1)

1. From the project list, open one of the 3 seeded sample projects.
2. Confirm the board shows exactly 4 columns: To Do, In Progress, In Review, Done (see [contracts/tasks-api.md](./contracts/tasks-api.md)).
3. Drag a task from To Do to In Progress, then to In Review, then to Done.
4. Reload the page (or open the board in a second browser tab) and confirm the task appears in Done with its title, assignee, and comments unchanged — this also proves the [SignalR real-time broadcast](./contracts/notifications-api.md) delivered the move to the second client.

## Validate User Story 2 — Create and assign project work (P2)

1. From the project list, create a new project with a unique, non-empty name; confirm it opens as its own empty (4-column) board.
2. On a board, create a task with a title and select one of the 5 predefined users as assignee; confirm it appears in To Do with that assignee.
3. Attempt to create a project with a blank/whitespace-only name and a task with a blank title; confirm both are rejected with a visible required-field message and no item is created ([contracts/projects-api.md](./contracts/projects-api.md), [contracts/tasks-api.md](./contracts/tasks-api.md)).

## Validate User Story 3 — Discuss task context (P3)

1. Open an existing task and add a non-empty comment as any predefined user; confirm it displays with author and timestamp.
2. Reopen the task and confirm all comments render in chronological order.
3. Attempt to submit an empty comment; confirm it is rejected and no comment is added.

## Run automated checks

```bash
dotnet test
```

Runs xUnit contract tests for each API (`Taskify.ProjectsApi.Tests`, `Taskify.TasksApi.Tests`, `Taskify.NotificationsApi.Tests`), bUnit component tests (`Taskify.Web.Tests`), and the `Aspire.Hosting.Testing` cross-service integration suite (`Taskify.AppHost.Tests`) covering the create → move → notify flow end to end.

## Reference

- Data shapes: [data-model.md](./data-model.md)
- API contracts: [contracts/projects-api.md](./contracts/projects-api.md), [contracts/tasks-api.md](./contracts/tasks-api.md), [contracts/notifications-api.md](./contracts/notifications-api.md)
- Design rationale: [research.md](./research.md)
