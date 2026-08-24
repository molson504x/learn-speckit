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

The solution is currently at the foundational phase. Shared service defaults, reference data, Aspire orchestration, SQLite-backed service shells, and typed web clients are in place; feature behavior and UI workflows are implemented in later phases described by [tasks.md](specs/001-taskify-kanban/tasks.md).

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