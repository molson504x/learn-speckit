# Contract: Projects API (`Taskify.ProjectsApi`)

Base path: `/api/projects`

## `GET /api/projects`

Returns all projects (seed data includes exactly 3 on first run).

**200 OK**
```json
[
  { "id": "guid", "name": "string", "createdAtUtc": "2026-08-23T00:00:00Z" }
]
```

## `GET /api/projects/{id}`

**200 OK** — single project as above.
**404 Not Found** — no project with `id`.

## `POST /api/projects`

**Request**
```json
{ "name": "string", "createdByUserId": "guid" }
```

- `name` required, trimmed non-empty, unique (case-insensitive) across all projects (FR-004, FR-012).
- `createdByUserId` required, must be one of the 5 predefined user IDs.

**201 Created** — project as above, `Location` header to `GET /api/projects/{id}`.

**400 Bad Request**
```json
{ "errors": { "name": ["Project name is required."] } }
```
Returned for empty/whitespace name, duplicate name, or unknown `createdByUserId`. No project is created (FR-012).

## `GET /api/projects/{id}/exists`

Internal endpoint used by `Taskify.TasksApi` to validate `ProjectId` on task create without a shared database.

**200 OK** `{ "exists": true }` / **200 OK** `{ "exists": false }`
