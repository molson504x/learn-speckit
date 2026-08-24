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

The solution is currently at the phase 1 setup stage. The project shells and shared
configuration are in place; API behavior, persistence, orchestration, and UI features
are implemented in later phases described by [tasks.md](specs/001-taskify-kanban/tasks.md).

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