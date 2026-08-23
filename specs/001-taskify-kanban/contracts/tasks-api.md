# Contract: Tasks API (`Taskify.TasksApi`)

Base path: `/api/tasks`

## `GET /api/tasks?projectId={projectId}`

Returns all tasks for a project, each including its comments, grouped implicitly by `column` for board rendering.

**200 OK**
```json
[
  {
    "id": "guid",
    "projectId": "guid",
    "title": "string",
    "description": "string|null",
    "assigneeUserId": "guid|null",
    "column": "ToDo|InProgress|InReview|Done",
    "createdAtUtc": "2026-08-23T00:00:00Z",
    "comments": [
      { "id": "guid", "authorUserId": "guid", "text": "string", "createdAtUtc": "2026-08-23T00:00:00Z" }
    ]
  }
]
```
Comments are always ordered ascending by `createdAtUtc` (FR-011). An empty project returns `[]` (edge case: board still renders all 4 empty columns client-side).

## `POST /api/tasks`

**Request**
```json
{ "projectId": "guid", "title": "string", "description": "string|null", "assigneeUserId": "guid|null", "actingUserId": "guid" }
```

- `projectId` required, must exist (validated via Projects API `GET /api/projects/{id}/exists`).
- `title` required, trimmed non-empty (FR-005, FR-012).
- `assigneeUserId`, if present, must be one of the 5 predefined users (FR-005, FR-013).
- `actingUserId` required; the service validates it against the 5 predefined users for this mutation.
- New task is created with `column = ToDo` (FR-006).

**201 Created** — task as above (no comments yet).
**400 Bad Request** — missing/whitespace title, unknown `projectId`, or invalid `assigneeUserId`; no partial write (FR-012, FR-013).

## `PATCH /api/tasks/{id}/column`

Moves a task to a different Kanban column.

**Request**
```json
{ "column": "ToDo|InProgress|InReview|Done", "actingUserId": "guid" }
```

**200 OK** — updated task. All other fields (title, description, assignee, comments, projectId) unchanged (FR-009).
**404 Not Found** — unknown task id.
**400 Bad Request** — `column` not one of the 4 valid values.
- `actingUserId` is required and must be one of the 5 predefined users.

## `PATCH /api/tasks/{id}/assignee`

Assigns or reassigns a task (FR-011A).

**Request**
```json
{ "assigneeUserId": "guid|null", "actingUserId": "guid" }
```

**200 OK** — updated task.
**400 Bad Request** — `assigneeUserId` not one of the 5 predefined users.
**404 Not Found** — unknown task id.
- `actingUserId` is required and must be one of the 5 predefined users.

## `POST /api/tasks/{id}/comments`

**Request**
```json
{ "authorUserId": "guid", "text": "string" }
```

- `text` required, trimmed non-empty (FR-010, FR-012).
- `authorUserId` required, represents the selected active user, and must be one of the 5 predefined users; the service validates it at the mutation boundary.

**201 Created** — comment as in the task's `comments` array.
**400 Bad Request** — empty/whitespace text or invalid author; no comment is added (FR-012).
**404 Not Found** — unknown task id.

## `GET /api/tasks/{id}/comments`

Returns all comments for a task, chronologically ordered (FR-011).

**200 OK** — array of comments (see shape above). Empty array when no comments exist (edge case: UI shows "no discussion yet").
