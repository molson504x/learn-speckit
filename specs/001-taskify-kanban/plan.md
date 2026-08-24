# Implementation Plan: Taskify Kanban Workspace

**Branch**: `001-taskify-kanban` | **Date**: 2026-08-23 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-taskify-kanban/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Taskify lets five predefined users (one PM, four engineers) create projects, create and assign tasks, move tasks across four Kanban columns, and comment on tasks — with no login required. The technical approach delivers this as a .NET Aspire-orchestrated distributed application: three ASP.NET Core minimal-API services (Projects, Tasks, Notifications) each owning a SQLite database, a Blazor Server frontend with drag-and-drop Kanban boards, and SignalR-based real-time propagation of board changes (task moves, assignments, new comments) surfaced through the Notifications service.

## Technical Context

**Language/Version**: C# 13 / .NET 9 (.NET Aspire 9.x)

**Primary Dependencies**: .NET Aspire (AppHost + ServiceDefaults), ASP.NET Core Minimal APIs, EF Core 9 with `Microsoft.EntityFrameworkCore.Sqlite`, ASP.NET Core SignalR, Blazor Server, SortableJS (via JS interop) for drag-and-drop board columns

**Storage**: SQLite — one database file per owning service (`projects.db`, `tasks.db`, `notifications.db`), consistent with the constitution's per-service data ownership rule

**Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) for API contract tests, `Aspire.Hosting.Testing` for distributed-app integration tests, bUnit for Blazor component tests

**Target Platform**: Cross-platform ASP.NET Core services running as Aspire-orchestrated containers/processes (Linux server in production, local dev via Aspire AppHost dashboard)

**Project Type**: Web application — multi-service (Aspire-orchestrated backend services + Blazor Server frontend)

**Performance Goals**: Board interactions (task move, assign, comment) render optimistically within 200ms; real-time board updates propagate to other connected clients within 1 second via SignalR

**Validation Protocol**: Use a clean seeded workspace and a stopwatch across five representative users; the acceptance threshold is four of five users completing board-open, create+assign+move, and comment flows within the time limits specified in SC-001 to SC-003. This protocol is part of the feature acceptance gate, not an implementation detail.

**Constraints**: No user authentication/login in this phase (predefined-user selection only, per FR-002/FR-014); every mutation carries and server-validates the selected predefined acting-user ID; concurrent updates to the same task and keyboard task movement are out of scope for this phase; SQLite is single-writer per database, so each service's write volume must stay within SQLite's concurrency limits for the 5-user/3-project scale; must run fully via `dotnet run` on the Aspire AppHost for local development

**Scale/Scope**: 5 predefined users, 3 seed projects, 4 fixed Kanban columns, small demo/internal scale (not designed for public multi-tenant load)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Security-First**: This feature intentionally has no login (FR-002, FR-014), which is a partial deviation from "authorization MUST be enforced server-side at every protected boundary." This is a scoped internal-first-phase exception, not a general policy change. The project must treat it as a release-gate exception requiring explicit review before broader rollout, while keeping all write endpoints validated against the five predefined users server-side. PASS with documented exception and governance note.
- **Validated Inputs**: All service boundaries (Projects API, Tasks API, Notifications API) validate project name / task title / comment text as non-empty after trim, validate assignee and comment author against the 5 predefined user IDs, and return structured 400 errors without partial writes. PASS.
- **Service Boundaries**: Each service (Projects, Tasks, Notifications) owns its own SQLite database and exposes only its versioned REST contract; no service reaches into another's database. Cross-service reads (e.g., Tasks API needing project existence) go through the Projects API contract, not direct DB access. PASS.
- **Documented Code**: Each service's public endpoints, the predefined-user assumption, and the no-auth boundary will be documented in contracts/ and the service READMEs generated during implementation. PASS (tracked as an implementation obligation).
- **Verifiable Delivery**: Contract tests per service, integration tests for cross-service task/comment/notification flows, and bUnit tests for the board's drag-and-drop and real-time update behavior are planned in Phase 1 outputs and will be enumerated as tasks in `/speckit-tasks`. PASS.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

```text
src/
├── Taskify.AppHost/              # .NET Aspire orchestrator (service graph, SQLite volumes, dashboard)
├── Taskify.ServiceDefaults/      # Shared Aspire telemetry/health/resilience defaults + predefined-user reference data
├── Taskify.ProjectsApi/          # REST API owning Project data (SQLite: projects.db)
│   ├── Endpoints/
│   ├── Models/
│   └── Data/
├── Taskify.TasksApi/             # REST API owning Task, Comment, KanbanColumn data (SQLite: tasks.db)
│   ├── Endpoints/
│   ├── Models/
│   └── Data/
├── Taskify.NotificationsApi/     # REST API + SignalR hub for real-time board events (SQLite: notifications.db)
│   ├── Endpoints/
│   ├── Hubs/
│   └── Data/
└── Taskify.Web/                  # Blazor Server frontend: project list, Kanban board, drag-and-drop, SignalR client
    ├── Components/
    ├── Pages/
    └── Services/                 # Typed HTTP clients for Projects/Tasks/Notifications APIs

tests/
├── Taskify.ProjectsApi.Tests/    # Contract + unit tests
├── Taskify.TasksApi.Tests/       # Contract + unit tests
├── Taskify.NotificationsApi.Tests/
├── Taskify.Web.Tests/            # bUnit component tests (board rendering, drag-and-drop, live updates)
└── Taskify.AppHost.Tests/        # Aspire.Hosting.Testing integration tests across the full service graph
```

**Structure Decision**: Web application, Option 2 variant, split into three Aspire-managed backend services instead of one to satisfy the constitution's Service Boundaries principle (each service owns its data and contract). `Taskify.AppHost` wires up the four projects plus their SQLite storage; `Taskify.Web` is the single Blazor Server frontend consuming all three REST APIs and subscribing to the Notifications SignalR hub for real-time board updates.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| No authentication/authorization at API boundaries (Security-First) | Spec (FR-002, FR-014) explicitly requires a credential-free first phase where any predefined user may act as any identity | Adding login would contradict FR-002/FR-014 and the spec's "no login" scope; server-side identity validation against the 5 predefined users is retained as a partial mitigation |
| Three services instead of one deployable unit (added operational surface) | Constitution's Service Boundaries principle requires each service to own its data and expose a versioned contract | A single monolithic API would simplify deployment but would violate the constitution's data-ownership and contract-isolation requirements |
