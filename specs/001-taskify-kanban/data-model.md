# Data Model: Taskify Kanban Workspace

## PredefinedUser *(shared reference data, seeded in `Taskify.ServiceDefaults`, read by Projects & Tasks APIs)*

| Field | Type | Rules |
|-------|------|-------|
| `Id` | GUID (fixed, seeded) | Immutable, one of exactly 5 seed values |
| `Name` | string | Non-empty, distinct across the roster |
| `Role` | enum (`ProductManager`, `Engineer`) | Exactly 1 `ProductManager` and 4 `Engineer` values in the roster |

- No create/update/delete operations — the roster is fixed seed data per FR-001.
- Referenced by ID from `Project.CreatedByUserId`, `Task.AssigneeUserId`, and `Comment.AuthorUserId`.

## Project *(owned by Taskify.ProjectsApi, stored in projects.db)*

| Field | Type | Rules |
|-------|------|-------|
| `Id` | GUID | Primary key, generated on create |
| `Name` | string | Required; trimmed non-empty (FR-012); unique across all projects (Assumptions) |
| `CreatedByUserId` | GUID | Required; selected acting user, validated against the five predefined users |
| `CreatedAtUtc` | DateTime | Set on create, immutable |

- Seed data: exactly 3 sample projects created at first run (FR-003).
- Relationships: a project has many tasks (owned by Tasks API, related by `ProjectId`, not a DB foreign key).
- Validation: reject create when `Name` is empty/whitespace or already exists (case-insensitive) → 400 with a field-level error, no partial write (FR-004, FR-012).
- `CreatedByUserId` is immutable provenance, not an audit-history feature.

## Task *(owned by Taskify.TasksApi, stored in tasks.db)*

| Field | Type | Rules |
|-------|------|-------|
| `Id` | GUID | Primary key, generated on create |
| `ProjectId` | GUID | Required; must reference an existing project (validated via Projects API) |
| `Title` | string | Required; trimmed non-empty (FR-005, FR-012) |
| `Description` | string? | Optional |
| `AssigneeUserId` | GUID? | Optional; if set, must be one of the 5 predefined users (FR-005, FR-013) |
| `Column` | enum `KanbanColumn` (`ToDo`, `InProgress`, `InReview`, `Done`) | Required; defaults to `ToDo` on create (FR-006); exactly one column at a time (FR-007, edge case) |
| `CreatedAtUtc` | DateTime | Set on create, immutable |

- State transitions: `Column` may move freely between any of the 4 values by any predefined user (FR-008); all other fields (`Title`, `Description`, `AssigneeUserId`, comments, `ProjectId`) are preserved across a column change (FR-009).
- Validation: reject create/update when `Title` is empty/whitespace, `ProjectId` doesn't exist, or `AssigneeUserId` is not one of the 5 predefined users → 400, no partial write (FR-012, FR-013).
- Relationships: a task has many comments (same service/DB); a task belongs to one project (cross-service reference by `ProjectId`).

## Comment *(owned by Taskify.TasksApi, stored in tasks.db, child of Task)*

| Field | Type | Rules |
|-------|------|-------|
| `Id` | GUID | Primary key, generated on create |
| `TaskId` | GUID | Required; must reference an existing task in the same store |
| `AuthorUserId` | GUID | Required; must be one of the 5 predefined users (FR-010) |
| `Text` | string | Required; trimmed non-empty (FR-010, FR-012) |
| `CreatedAtUtc` | DateTime | Set on create, immutable; used for chronological ordering (FR-011) |

- Validation: reject create when `Text` is empty/whitespace or `AuthorUserId` is not a predefined user → 400, no partial write.
- Ordering: comments are always returned sorted ascending by `CreatedAtUtc` for a given task (FR-011).

## NotificationEvent *(owned by Taskify.NotificationsApi, stored in notifications.db)*

| Field | Type | Rules |
|-------|------|-------|
| `Id` | GUID | Primary key, generated on receipt |
| `ProjectId` | GUID | Required; used to group SignalR broadcast recipients by open board |
| `EventType` | enum (`TaskCreated`, `TaskMoved`, `TaskAssigned`, `CommentAdded`) | Required |
| `Payload` | JSON string | Serialized event details (task id, from/to column, assignee, comment id, etc.) |
| `OccurredAtUtc` | DateTime | Set when the Tasks/Projects API reports the event |

- Write path: Tasks API (and Projects API, for project creation) POSTs an event after each successful mutation; Notifications API persists it (for late-joining clients / audit) and immediately broadcasts it over `BoardHub` to clients in the matching `projectId` SignalR group.
- Read path: `GET /api/notifications/events?projectId=` supports fetching recent events (e.g., for a client reconnecting) in addition to the live SignalR stream.

## Kanban Column *(fixed enumeration, not a persisted entity)*

Exactly four values, in workflow order: `ToDo`, `InProgress`, `InReview`, `Done` (FR-007). Represented as a shared enum in `Taskify.ServiceDefaults` so Projects/Tasks/Web all agree on names and ordering.
