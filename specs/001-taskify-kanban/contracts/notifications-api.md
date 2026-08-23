# Contract: Notifications API (`Taskify.NotificationsApi`)

Base path: `/api/notifications`

## `POST /api/notifications/events` *(internal — called by Projects/Tasks APIs)*

Records a board-change event and immediately broadcasts it to SignalR clients in the matching project group.

**Request**
```json
{
  "projectId": "guid",
  "eventType": "TaskCreated|TaskMoved|TaskAssigned|CommentAdded",
  "payload": { "...": "event-specific details, e.g. taskId, fromColumn, toColumn, assigneeUserId, commentId" }
}
```

**202 Accepted** — event recorded and broadcast triggered (fire-and-forget from the caller's perspective; failures here must not block or roll back the originating write in Tasks/Projects API).

## `GET /api/notifications/events?projectId={projectId}&since={isoTimestamp?}`

Returns recent events for a project, for clients that need to resync after reconnecting (e.g., missed SignalR messages).

**200 OK**
```json
[
  { "id": "guid", "projectId": "guid", "eventType": "TaskMoved", "payload": { "...": "..." }, "occurredAtUtc": "2026-08-23T00:00:00Z" }
]
```

## SignalR Hub: `/hubs/board`

- Clients call `JoinProject(projectId: string)` after connecting to join the SignalR group for the board they have open.
- Server pushes one of the following client methods whenever a matching `POST /api/notifications/events` is processed:
  - `TaskCreated(payload)`
  - `TaskMoved(payload)`
  - `TaskAssigned(payload)`
  - `CommentAdded(payload)`
- `Taskify.Web` calls `LeaveProject(projectId: string)` when navigating away from a board to stop receiving that project's events.
