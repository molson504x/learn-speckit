# Taskify Kanban

Taskify is a .NET Aspire-based Kanban workspace for five predefined team members.
The feature design is documented in [specs/001-taskify-kanban](specs/001-taskify-kanban/).

## Repository Structure

- `src/Taskify.AppHost` - Aspire application host
- `src/Taskify.ServiceDefaults` - shared service configuration and reference data
- `src/Taskify.ProjectsApi` - project service
- `src/Taskify.TasksApi` - task and comment service
- `src/Taskify.NotificationsApi` - notification and SignalR service
- `src/Taskify.Web` - Blazor frontend

The solution now includes the Phase 3 MVP board workflow:

- `Taskify.ProjectsApi` seeds and serves three sample projects: `Q4 Roadmap`, `Bug Triage`, and `Research Sprint`
- `Taskify.TasksApi` seeds board tasks across `ToDo`, `InProgress`, `InReview`, and `Done`, and preserves task details when tasks move between columns
- `Taskify.NotificationsApi` records board events and broadcasts `TaskMoved` updates over SignalR
- `Taskify.Web` renders the project list, four-column board, and drag-and-drop task moves for any predefined active user

The shared `Taskify.ServiceDefaults` project owns the predefined user roster, Kanban column enum, and input validation helpers so every service validates against the same rules without sharing private storage.

## Prerequisites

- .NET 9 SDK
- Docker Desktop or Podman when required by the Aspire hosting environment

## Build

From the repository root:

```bash
dotnet restore Taskify.slnx
dotnet build Taskify.slnx --configuration Release --no-restore
```

Run the automated checks with:

```bash
dotnet test Taskify.slnx
```

The same restore and release build commands run in [CI](.github/workflows/ci.yml).

## Board Workflow

1. Open the project list and choose one of the seeded projects.
2. Select an active predefined user.
3. Drag a task card between the four Kanban columns.
4. The Tasks API persists the move, the Notifications API broadcasts the `TaskMoved` event, and connected board views update without a full-page reload.