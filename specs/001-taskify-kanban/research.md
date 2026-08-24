# Phase 0 Research: Taskify Kanban Workspace

## 1. Service topology under .NET Aspire

- **Decision**: Model Taskify as an Aspire application with four projects — `Taskify.AppHost`, `Taskify.ServiceDefaults`, three REST API services (`Taskify.ProjectsApi`, `Taskify.TasksApi`, `Taskify.NotificationsApi`), and one Blazor Server frontend (`Taskify.Web`). The AppHost declares each API as a resource with its own SQLite file (via `AddSqlite`/container volume or local file mount) and wires the Web project's typed `HttpClient`s and SignalR connection to the API resources by service discovery name.
- **Rationale**: Aspire's resource model gives local-dev orchestration (single `dotnet run` on AppHost), built-in service discovery, health checks, and OpenTelemetry via `ServiceDefaults`, while keeping each service's data and contract independently owned per the constitution's Service Boundaries principle.
- **Alternatives considered**:
  - *Single monolithic ASP.NET Core app*: simpler to build and deploy, but violates Service Boundaries (all data in one store, no independent contract versioning) and doesn't reflect the requested "REST APIs for projects, tasks, and notifications" as distinct capabilities.
  - *Full microservices with a message broker (e.g., RabbitMQ/Azure Service Bus) between services*: more resilient at scale, but adds infrastructure the 5-user/3-project scale doesn't justify; deferred as a future evolution, not needed for this phase.

## 2. Data storage per service

- **Decision**: Each service owns a dedicated SQLite database file managed via EF Core 9 (`Microsoft.EntityFrameworkCore.Sqlite`), migrated at startup with `Database.Migrate()`. Files are placed in a per-environment data directory Aspire mounts as a volume so state survives restarts during local development.
- **Rationale**: SQLite satisfies the "SQLite" requirement in the user's prompt, needs no separate DB server, and its single-writer model is not a concern at 5-user scale. Per-service files enforce the constitution's data-ownership rule without extra infrastructure.
- **Alternatives considered**: One shared SQLite file across services (rejected — violates Service Boundaries: a service must own its data, not share a file with others); PostgreSQL per service (rejected — user explicitly asked for SQLite; unnecessary operational weight for this scale).

## 3. Cross-service data relationships (Task → Project, Task → User)

- **Decision**: Tasks reference `ProjectId` and `AssigneeUserId`/`CommentAuthorUserId` by value (GUID/int), not by foreign key into another service's database. The Tasks API validates a `ProjectId` by calling the Projects API's contract (or a cached lookup) before creating a task; predefined-user IDs are validated against a shared, versioned reference dataset and validation helper in `Taskify.ServiceDefaults` (static seed list of the 5 users), since users are fixed, read-only reference data with no service of their own.
- **Rationale**: Keeps each service authoritative over only its own entities while still enforcing FR-005 (valid assignee), FR-011A, and FR-013 (no assignment outside the 5 users) at the boundary that needs it, in line with Validated Inputs and Service Boundaries.
- **Alternatives considered**: A separate Users microservice (rejected as over-engineering — there are exactly 5 immutable, seed-only users with no lifecycle operations required by the spec); direct cross-service DB joins (rejected — explicitly prohibited by the constitution).

## 4. Real-time updates

- **Decision**: `Taskify.NotificationsApi` hosts a SignalR hub (`BoardHub`) that broadcasts board-change events (`TaskMoved`, `TaskCreated`, `TaskAssigned`, `CommentAdded`) to all connected clients grouped by `projectId`. The Tasks API publishes these events to the Notifications API over an internal REST call (`POST /api/notifications/events`) after each successful write; `Taskify.Web` connects to the hub per open board and updates the Blazor UI in real time.
- **Rationale**: Satisfies "real-time updates" from the prompt using a technology already in the ASP.NET Core/Blazor Server stack, avoiding a new message-broker dependency. Routing events through a REST endpoint (rather than the Tasks API embedding SignalR directly) preserves Service Boundaries — Notifications owns the real-time broadcast concern.
- **Alternatives considered**: Polling from the Blazor client (rejected — higher latency, contradicts "real-time"); Tasks API hosting its own SignalR hub directly (rejected — blurs the requested "notifications" service boundary and duplicates hub logic if other services need to notify later).

## 5. Blazor Server drag-and-drop board

- **Decision**: Use SortableJS via a small JS-interop wrapper component for column-to-column drag-and-drop of task cards; on `drop`, the Blazor component calls the Tasks API's `PATCH /api/tasks/{id}/column` endpoint, then relies on the SignalR broadcast (plus local optimistic update) to reflect the change everywhere.
- **Rationale**: SortableJS is a lightweight, widely used, dependency-free JS library that integrates cleanly with Blazor Server's JS interop and doesn't require a full component framework like MudBlazor for this single interaction.
- **Alternatives considered**: HTML5 native drag-and-drop APIs directly (rejected — inconsistent touch-device support, more custom JS to write and maintain); a full component library like MudBlazor/Telerik (rejected — pulls in a large dependency for one interaction pattern).

## 6. Testing approach

- **Decision**: xUnit + `WebApplicationFactory` for per-service REST contract tests (request/response shape, validation, status codes); `Aspire.Hosting.Testing` (`DistributedApplicationTestingBuilder`) for integration tests that boot the whole app graph and verify cross-service flows (create task → notification broadcast); bUnit for Blazor component behavior (board rendering, drag handlers, SignalR message handling).
- **Rationale**: Matches the constitution's Verifiable Delivery principle (contract, validation, and failure-path coverage) using first-party, well-supported .NET testing tools for each layer.
- **Alternatives considered**: End-to-end browser tests (e.g., Playwright) for drag-and-drop (deferred — valuable but heavier; can be added later as an enhancement, not required to prove the acceptance scenarios which are testable at the component/API level).

## Outcome

All technical unknowns are resolved; no `NEEDS CLARIFICATION` markers remain in the Technical Context. The clarified first-phase boundaries are server-side validation of every mutation's predefined acting-user ID, collective (not per-project) sample-data coverage, no required concurrent-update resolution, and implementation-defined accessibility behavior for task movement.
