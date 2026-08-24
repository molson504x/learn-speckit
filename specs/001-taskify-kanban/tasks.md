# Tasks: Taskify Kanban Workspace

**Input**: Design documents from `/specs/001-taskify-kanban/`

**Prerequisites**: plan.md ✓, spec.md ✓, data-model.md ✓, contracts/ ✓, research.md ✓, quickstart.md ✓

**Tests**: Contract tests (xUnit + WebApplicationFactory), integration tests (Aspire.Hosting.Testing), and component tests (bUnit) are included per user story. Tests MUST be written FIRST and FAIL before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story. All three stories can run in parallel after the Foundational phase completes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Includes exact file paths in descriptions

## Path Conventions

- **Backend services**: `src/Taskify.{ServiceName}/`
- **Frontend**: `src/Taskify.Web/`
- **Tests**: `tests/Taskify.{ProjectName}.Tests/`
- **Aspire orchestrator**: `src/Taskify.AppHost/`
- **Shared defaults**: `src/Taskify.ServiceDefaults/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create solution structure with projects: Taskify.AppHost, Taskify.ServiceDefaults, Taskify.ProjectsApi, Taskify.TasksApi, Taskify.NotificationsApi, Taskify.Web
- [x] T002 [P] Add .NET 9 / Aspire 9.x dependencies to all service projects via .csproj
- [x] T003 [P] Configure linting and formatting (StyleCop, EditorConfig) in solution root
- [x] T004 [P] Create .gitignore and .github/workflows for CI/CD pipeline scaffolding
- [x] T005 Create src/ directory structure per project organization (Endpoints/, Models/, Data/, Components/, Pages/, Services/, Hubs/) and update README.md plus specs/001-taskify-kanban/quickstart.md with the initial architecture and local run notes

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Shared Reference Data & Aspire Orchestration

- [X] T006 [P] Define PredefinedUser entity and seeded roster (5 users: Maya Chen PM, Jordan Lee Engineer, Priya Shah Engineer, Luis Garcia Engineer, Ava Williams Engineer) in `src/Taskify.ServiceDefaults/ReferenceData/PredefinedUsers.cs`
- [X] T007 [P] Create `Taskify.AppHost` Aspire AppHost project with service discovery configuration and resource definitions in `src/Taskify.AppHost/Program.cs`
- [X] T008 [P] Configure SQLite data volumes and mounts for each service database in AppHost (`projects.db`, `tasks.db`, `notifications.db`)
- [X] T009 [P] Implement `Taskify.ServiceDefaults` project with OpenTelemetry, health checks, and resilience middleware extensions in `src/Taskify.ServiceDefaults/Extensions.cs`
- [X] T010 [P] Create shared KanbanColumn enum (ToDo, InProgress, InReview, Done) in `src/Taskify.ServiceDefaults/Models/KanbanColumn.cs`

### Projects API Foundation

- [X] T011 [P] Create Projects API ASP.NET Core project with minimal API structure in `src/Taskify.ProjectsApi/Program.cs`
- [X] T012 [P] Create EF Core DbContext `ProjectsDbContext` for projects.db with migrations in `src/Taskify.ProjectsApi/Data/ProjectsDbContext.cs`
- [X] T013 [P] Define Project model entity in `src/Taskify.ProjectsApi/Models/Project.cs` (Id, Name, CreatedByUserId, CreatedAtUtc)
- [X] T014 Create Projects API database initialization with EF Core migrations and seeding in `src/Taskify.ProjectsApi/Data/ProjectsSeeder.cs`

### Tasks API Foundation

- [X] T015 [P] Create Tasks API ASP.NET Core project with minimal API structure in `src/Taskify.TasksApi/Program.cs`
- [X] T016 [P] Create EF Core DbContext `TasksDbContext` for tasks.db with migrations in `src/Taskify.TasksApi/Data/TasksDbContext.cs`
- [X] T017 [P] Define Task model entity in `src/Taskify.TasksApi/Models/Task.cs` (Id, ProjectId, Title, Description, AssigneeUserId, Column, CreatedAtUtc)
- [X] T018 [P] Define Comment model entity in `src/Taskify.TasksApi/Models/Comment.cs` (Id, TaskId, AuthorUserId, Text, CreatedAtUtc)
- [X] T019 Create Tasks API database initialization with EF Core migrations in `src/Taskify.TasksApi/Data/TasksDbContext.cs` (no seed data until P2)

### Notifications API Foundation

- [X] T020 [P] Create Notifications API ASP.NET Core project with minimal API structure in `src/Taskify.NotificationsApi/Program.cs`
- [X] T021 [P] Create EF Core DbContext `NotificationsDbContext` for notifications.db with migrations in `src/Taskify.NotificationsApi/Data/NotificationsDbContext.cs`
- [X] T022 [P] Define NotificationEvent model entity in `src/Taskify.NotificationsApi/Models/NotificationEvent.cs` (Id, ProjectId, EventType, Payload, OccurredAtUtc)
- [X] T023 [P] Create SignalR BoardHub with JoinProject/LeaveProject methods in `src/Taskify.NotificationsApi/Hubs/BoardHub.cs`

### Frontend Foundation

- [X] T024 [P] Create Blazor Server project structure with App.razor and MainLayout in `src/Taskify.Web/`
- [X] T025 [P] Create typed HttpClient services for ProjectsApi, TasksApi, NotificationsApi in `src/Taskify.Web/Services/`
- [X] T026 [P] Configure Blazor project to reference AppHost via Aspire service discovery in `src/Taskify.Web/Program.cs`
- [X] T027 Create PredefinedUser selection component in `src/Taskify.Web/Components/UserSelector.razor` (displays 5 users, saves selection to session)

### Service Inter-Communication

- [X] T028 [P] Configure HttpClient factory and service discovery names in each API Program.cs for cross-service calls
- [X] T029 [P] Add shared validation utilities for predefined-user ID and input validation in `src/Taskify.ServiceDefaults/Validation/` and update README.md plus specs/001-taskify-kanban/research.md / data-model.md with the shared validation and service-boundary decisions

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Track Work on a Project Board (Priority: P1) 🎯 MVP

**Goal**: Display a project's board with tasks in exactly four Kanban columns (ToDo, InProgress, InReview, Done), and allow any predefined user to move tasks between columns while preserving all task details (title, assignee, comments).

**Independent Test**: Open a sample project, move a task from ToDo through each subsequent column, and verify the task appears in its new column with its details intact; repeat with each of the five predefined users.

### Contract Tests for User Story 1 ⚠️

- [ ] T030 [P] [US1] Write failing contract test for `GET /api/projects` in `tests/Taskify.ProjectsApi.Tests/ProjectsApiTests.cs` (verify 3 projects returned with correct shape)
- [ ] T031 [P] [US1] Write failing contract test for `GET /api/tasks?projectId={id}` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify tasks array shape with comments sorted by createdAtUtc)
- [ ] T032 [P] [US1] Write failing contract test for `PATCH /api/tasks/{id}/column` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify task moves to new column, other fields unchanged)
- [ ] T033 [P] [US1] Write failing integration test for task move propagation via SignalR in `tests/Taskify.AppHost.Tests/BoardRealTimeTests.cs` (verify TaskMoved event broadcasts to all connected clients)
- [ ] T034 [P] [US1] Write failing bUnit component test for Kanban board rendering in `tests/Taskify.Web.Tests/KanbanBoardTests.cs` (verify 4 columns render, tasks appear in correct columns; include edge case: empty project returns no tasks but all 4 columns still render, no load failure)
- [ ] T035 [P] [US1] Write failing bUnit component test for task drag-and-drop behavior in `tests/Taskify.Web.Tests/DragDropTests.cs` (verify SortableJS interop calls correct API endpoint)

### Implementation for User Story 1

#### Projects API - Seed Sample Projects

- [ ] T036 Create Projects API seeding to generate exactly 3 sample projects on first run in `src/Taskify.ProjectsApi/Data/ProjectsSeeder.cs` (tasks seeded separately by T040). Sample project names should be distinct and representative (e.g., "Q4 Roadmap", "Bug Triage", "Research Sprint") to aid testing and demos. Reference: see `specs/001-taskify-kanban/design-guide.md` for naming guidance.
- [ ] T037 [P] [US1] Implement `GET /api/projects` endpoint in `src/Taskify.ProjectsApi/Endpoints/ProjectsEndpoints.cs` (returns array of all projects)
- [ ] T038 [P] [US1] Implement `GET /api/projects/{id}` endpoint in `src/Taskify.ProjectsApi/Endpoints/ProjectsEndpoints.cs` (returns single project or 404)

#### Tasks API - Retrieve & Move Tasks

- [ ] T039 [P] [US1] Implement `GET /api/tasks?projectId={projectId}` endpoint in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs` (returns all tasks with comments for a project, sorted by column and createdAtUtc)
- [ ] T040 [US1] Seed Tasks API with sample tasks distributed across 4 columns (ToDo, InProgress, InReview, Done) in `src/Taskify.TasksApi/Data/TasksSeeder.cs` (depends on T036: must reference same projects). Ensure sample data validation: across all 3 projects collectively, at least 1 task in each column and at least 1 task with a comment. See `scripts/usability-test-script.md` for test data requirements.
- [ ] T041 [US1] Implement task-move validation: verify `PATCH /api/tasks/{id}/column` accepts only valid column values and validates `actingUserId` in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`
- [ ] T042 [US1] Implement `PATCH /api/tasks/{id}/column` endpoint to move task and preserve all other fields (title, description, assignee, comments, projectId) in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`
- [ ] T043 [US1] Emit TaskMoved event to Notifications API after successful column change in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs` (fire-and-forget; log failures but do not block the response)

#### Notifications API - Broadcast Board Changes

- [ ] T044 [P] [US1] Implement `POST /api/notifications/events` endpoint to record event and immediately broadcast via SignalR in `src/Taskify.NotificationsApi/Endpoints/NotificationsEndpoints.cs`
- [ ] T045 [US1] Implement `BoardHub.TaskMoved` SignalR client method to push move events to all connected clients in matching `projectId` group in `src/Taskify.NotificationsApi/Hubs/BoardHub.cs`
- [ ] T046 [P] [US1] Implement `GET /api/notifications/events?projectId={projectId}` endpoint for late-joining clients in `src/Taskify.NotificationsApi/Endpoints/NotificationsEndpoints.cs`

#### Frontend - Kanban Board Display & Interaction

- [ ] T047 [US1] Create Kanban board Blazor component in `src/Taskify.Web/Components/KanbanBoard.razor` that renders 4 columns with task cards (depends on T039, T043)
- [ ] T048 [P] [US1] Implement SortableJS JS interop wrapper in `src/Taskify.Web/Services/DragDropService.cs` and `src/Taskify.Web/wwwroot/js/sortable-interop.js` for column-to-column drag-and-drop
- [ ] T049 [US1] Integrate SortableJS drop handler to call `PATCH /api/tasks/{id}/column` endpoint in `src/Taskify.Web/Components/KanbanBoard.razor` and emit local optimistic update
- [ ] T050 [US1] Connect Blazor component to SignalR hub in `src/Taskify.Web/Components/KanbanBoard.razor` to subscribe to TaskMoved and other board events (depends on T044, T045)
- [ ] T051 [US1] Implement real-time board update in `src/Taskify.Web/Components/KanbanBoard.razor` to reflect incoming SignalR TaskMoved events without full-page reload
- [ ] T052 [P] [US1] Create Projects List page in `src/Taskify.Web/Pages/ProjectList.razor` (displays all projects, link to open each project's board)

### Integration Checkpoint for User Story 1

- [ ] T053 [US1] Run all User Story 1 contract and integration tests; ensure 100% pass with sample-data seeding in place and update README.md plus specs/001-taskify-kanban/design-guide.md with the board workflow, sample data, and real-time behavior notes

**At this point, User Story 1 should be fully functional and testable independently:** 
- A user can see all projects
- A user can open a project board
- A user can move any task between any two columns
- Task details (title, assignee, comments) remain unchanged after a move
- All five predefined users can perform these actions
- Multiple clients see board updates in real time via SignalR

---

## Phase 4: User Story 2 - Create and Assign Project Work (Priority: P2)

**Goal**: Allow any predefined user to create a new project and create/assign tasks within it, with full input validation and field-level error messages. New tasks always start in To Do column.

**Independent Test**: Create a project, add a task with a title and description, assign it to one of the five predefined users, and verify it begins in To Do with the selected assignee. Attempt to create with empty/whitespace inputs and verify rejection without creating partial data.

### Contract Tests for User Story 2 ⚠️

- [ ] T054 [P] [US2] Write failing contract test for `POST /api/projects` in `tests/Taskify.ProjectsApi.Tests/ProjectsApiTests.cs` (verify 201 with Location header, reject empty/duplicate names with 400 field error, no partial write)
- [ ] T055 [P] [US2] Write failing contract test for `POST /api/tasks` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify 201 with column=ToDo, reject empty title/invalid projectId/invalid assignee with 400 field error, no partial write)
- [ ] T056 [P] [US2] Write failing contract test for `PATCH /api/tasks/{id}/assignee` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify 200 on valid assignee, reject invalid assignee with 400, allow null/reassignment as no-op)
- [ ] T057 [P] [US2] Write failing integration test for task creation event broadcasting in `tests/Taskify.AppHost.Tests/TaskCreationTests.cs` (verify TaskCreated event broadcasts via SignalR)
- [ ] T058 [P] [US2] Write failing bUnit component test for Create Project form in `tests/Taskify.Web.Tests/CreateProjectFormTests.cs` (verify form submission, error display on invalid input, field preservation)
- [ ] T059 [P] [US2] Write failing bUnit component test for Create Task form in `tests/Taskify.Web.Tests/CreateTaskFormTests.cs` (verify form submission, assignee dropdown shows 5 users, error display on invalid input)

### Implementation for User Story 2

#### Projects API - Create Projects

- [ ] T060 [US2] Implement input validation for project name (non-empty after trim, unique case-insensitive) in `src/Taskify.ProjectsApi/Validation/ProjectValidator.cs`
- [ ] T061 [US2] Implement validation for `createdByUserId` against 5 predefined users in `src/Taskify.ProjectsApi/Validation/ProjectValidator.cs`
- [ ] T062 [US2] Implement `POST /api/projects` endpoint to create project with validation in `src/Taskify.ProjectsApi/Endpoints/ProjectsEndpoints.cs` (return 201 with Location header or 400 with field-level errors)
- [ ] T063 [US2] Add error response DTO to match contract spec in `src/Taskify.ProjectsApi/Models/ErrorResponse.cs`

#### Tasks API - Create Tasks & Assign

- [ ] T064 [P] [US2] Implement input validation for task title (non-empty after trim) in `src/Taskify.TasksApi/Validation/TaskValidator.cs`
- [ ] T065 [P] [US2] Implement validation for `projectId` existence via call to Projects API in `src/Taskify.TasksApi/Services/ProjectValidationService.cs`
- [ ] T066 [P] [US2] Implement validation for `assigneeUserId` and `actingUserId` against 5 predefined users in `src/Taskify.TasksApi/Validation/TaskValidator.cs`
- [ ] T067 [US2] Implement `POST /api/tasks` endpoint to create task with default column=ToDo in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs` (return 201 or 400 with field-level errors)
- [ ] T068 [US2] Implement `PATCH /api/tasks/{id}/assignee` endpoint to assign/reassign/unassign tasks in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs` (allow null, allow no-op reassignment)
- [ ] T069 [US2] Emit TaskCreated event to Notifications API after successful task creation in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`
- [ ] T070 [US2] Emit TaskAssigned event to Notifications API after successful assignment change in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`

#### Notifications API - Event Broadcast

- [ ] T071 [P] [US2] Implement `BoardHub.TaskCreated` SignalR client method to broadcast new tasks to connected clients in matching project group in `src/Taskify.NotificationsApi/Hubs/BoardHub.cs`
- [ ] T072 [P] [US2] Implement `BoardHub.TaskAssigned` SignalR client method to broadcast assignment changes in `src/Taskify.NotificationsApi/Hubs/BoardHub.cs`

#### Frontend - Create Project & Task Forms

- [ ] T073 [US2] Create Create Project form component in `src/Taskify.Web/Components/CreateProjectForm.razor` (field for project name, submit button, error display, success navigation to new board)
- [ ] T074 [US2] Add Create Project button to Projects List page in `src/Taskify.Web/Pages/ProjectList.razor`
- [ ] T075 [US2] Create Create Task form component in `src/Taskify.Web/Components/CreateTaskForm.razor` (fields for title, optional description, assignee dropdown with 5 users, submit, error display)
- [ ] T076 [US2] Add Create Task button/form to Kanban Board component in `src/Taskify.Web/Components/KanbanBoard.razor` (appears above To Do column or in modal)
- [ ] T077 [US2] Subscribe to TaskCreated and TaskAssigned events in Kanban Board to update UI in real time in `src/Taskify.Web/Components/KanbanBoard.razor` (depends on T069, T070)
- [ ] T078 [US2] Implement form field error display (inline, preserves other entered values) in both Create Project and Create Task forms

### Integration Checkpoint for User Story 2

- [ ] T079 [US2] Run all User Story 2 contract and integration tests; ensure 100% pass and update README.md plus specs/001-taskify-kanban/quickstart.md with the project/task creation flow, validation examples, and role-based workflow notes

**At this point, User Stories 1 AND 2 should both work independently:**
- A user can create a new project with a unique non-empty name
- A user can create a task within a project with a non-empty title
- A user can assign a task to any of the 5 predefined users
- Newly created tasks appear in ToDo column
- New projects and tasks broadcast via SignalR for real-time updates on all connected clients
- All validation rejects invalid input without partial writes
- Field-level errors preserve other entered form values

---

## Phase 5: User Story 3 - Discuss Task Context (Priority: P3)

**Goal**: Allow any predefined user to add non-empty text comments to any task and read all comments for a task in chronological order with author and creation time.

**Independent Test**: Open an existing task, add a non-empty comment as a predefined user, then reopen the task and verify the comment displays its author, creation time, and is ordered chronologically with all other comments.

### Contract Tests for User Story 3 ⚠️

- [ ] T080 [P] [US3] Write failing contract test for `POST /api/tasks/{id}/comments` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify 201, reject empty/whitespace text with 400 field error, verify comment includes author and createdAtUtc)
- [ ] T081 [P] [US3] Write failing contract test for `GET /api/tasks/{id}/comments` in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs` (verify comments array sorted ascending by createdAtUtc, empty array when no comments)
- [ ] T082 [P] [US3] Write failing contract test that `GET /api/tasks?projectId={id}` includes all comments sorted by createdAtUtc in task response in `tests/Taskify.TasksApi.Tests/TasksApiTests.cs`
- [ ] T083 [P] [US3] Write failing integration test for comment creation event broadcasting in `tests/Taskify.AppHost.Tests/CommentTests.cs` (verify CommentAdded event broadcasts via SignalR)
- [ ] T084 [P] [US3] Write failing bUnit component test for Task Details component displaying comments in `tests/Taskify.Web.Tests/TaskDetailsTests.cs` (verify comments appear in chronological order, author and timestamp shown)
- [ ] T085 [P] [US3] Write failing bUnit component test for Add Comment form in `tests/Taskify.Web.Tests/AddCommentFormTests.cs` (verify form submission, error display on empty comment)

### Implementation for User Story 3

#### Tasks API - Comments CRUD

- [ ] T086 [P] [US3] Implement input validation for comment text (non-empty after trim) in `src/Taskify.TasksApi/Validation/CommentValidator.cs`
- [ ] T087 [US3] Implement `POST /api/tasks/{id}/comments` endpoint to create comment with validation in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs` (return 201 or 400 with field error, set createdAtUtc to now)
- [ ] T088 [P] [US3] Implement `GET /api/tasks/{id}/comments` endpoint returning array sorted ascending by createdAtUtc in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`
- [ ] T089 [US3] Ensure `GET /api/tasks?projectId={projectId}` includes comments array for each task, pre-sorted by createdAtUtc in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`
- [ ] T090 [US3] Emit CommentAdded event to Notifications API after successful comment creation in `src/Taskify.TasksApi/Endpoints/TasksEndpoints.cs`

#### Notifications API - Comment Broadcasting

- [ ] T091 [P] [US3] Implement `BoardHub.CommentAdded` SignalR client method to broadcast new comments to connected clients in matching project group in `src/Taskify.NotificationsApi/Hubs/BoardHub.cs`

#### Frontend - Task Details & Comments

- [ ] T092 [US3] Create Task Details component in `src/Taskify.Web/Components/TaskDetails.razor` (displays full task info: title, description, assignee, column, and comments section)
- [ ] T093 [US3] Implement Comments List section in Task Details to render all comments sorted chronologically with author name and timestamp in `src/Taskify.Web/Components/TaskDetails.razor`
- [ ] T094 [US3] Create Add Comment form component in `src/Taskify.Web/Components/AddCommentForm.razor` (text input, active-user selector, submit button, error display)
- [ ] T095 [US3] Integrate Add Comment form into Task Details component in `src/Taskify.Web/Components/TaskDetails.razor`
- [ ] T096 [US3] Add task click/detail view trigger to Kanban Board component to open Task Details modal/page in `src/Taskify.Web/Components/KanbanBoard.razor`
- [ ] T097 [US3] Subscribe to CommentAdded events in Task Details component to display new comments in real time in `src/Taskify.Web/Components/TaskDetails.razor` (depends on T090)
- [ ] T098 [US3] Handle real-time comment updates: append new comment to chronological list without full reload in `src/Taskify.Web/Components/TaskDetails.razor`

### Integration Checkpoint for User Story 3

- [ ] T099 [US3] Run all User Story 3 contract and integration tests; ensure 100% pass and update README.md plus the task details/comment documentation with the final comment workflow, author timestamps, and real-time behavior notes

**At this point, all three User Stories should work independently:**
- A user can add a non-empty comment to any task
- All comments display with their author name and creation timestamp
- Comments are always shown in chronological order (oldest first)

---

## Phase 6: Final Polish & Documentation Sync

**Purpose**: Final project validation and documentation consistency check before handoff

- [ ] T100 Final documentation pass to update README.md and all feature docs (specs/001-taskify-kanban/quickstart.md, specs/001-taskify-kanban/research.md, specs/001-taskify-kanban/design-guide.md, and any service/usage notes) with the final implementation, validation outcomes, sample data, and known limitations
- [ ] T101 Final validation sweep to confirm the README, quickstart, and feature documents match the shipped board workflow, task creation flow, comment flow, and local run instructions

- New comments broadcast via SignalR to all clients viewing the same task/board
- Empty comments are rejected without partial writes
- Field-level errors are displayed inline

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories, full-stack integration, and final validation

### Cross-Service Integration & Testing

- [ ] T100 [P] Run full Aspire.Hosting.Testing integration suite in `tests/Taskify.AppHost.Tests/` covering complete flows (create project → create task → move → comment → verify real-time propagation); include failure scenario: verify failed SignalR broadcasts do not block or rollback originating API mutations
- [ ] T101 [P] Verify AppHost boots all 4 services correctly with service discovery and health checks in `tests/Taskify.AppHost.Tests/`
- [ ] T102 [P] Test concurrent task moves from multiple simulated clients **on different tasks** and verify optimistic updates with SignalR synchronization in `tests/Taskify.AppHost.Tests/ConcurrencyTests.cs` (note: concurrent updates to the same task are out of scope per spec clarification)
- [ ] T103 Run full xUnit test suite for ProjectsApi, TasksApi, NotificationsApi; ensure all contract and validation tests pass

### Frontend Refinement

- [ ] T104 [P] Implement loading states and spinners during API calls and SignalR reconnects in all Blazor components (`src/Taskify.Web/Components/`)
- [ ] T105 [P] Add proper error handling and user-friendly error messages for network failures and API errors in `src/Taskify.Web/Services/`
- [ ] T106 [P] Implement SignalR reconnection logic with exponential backoff in `src/Taskify.Web/Services/`
- [ ] T107 [P] Add CSS styling for Kanban board, task cards, forms, and modals in `src/Taskify.Web/wwwroot/css/` with consistent theme and responsive layout. Reference: see `specs/001-taskify-kanban/design-guide.md` for color palette, component specifications, typography, and responsive breakpoints. Ensure all text meets minimum 4.5:1 contrast ratio (WCAG 2.1 Level AA).
- [ ] T108 Refine drag-and-drop visual feedback: highlight drop zones, show placeholder during drag, handle invalid drops in `src/Taskify.Web/Components/KanbanBoard.razor`
- [ ] T109 Implement task card preview/detail modal or side panel in `src/Taskify.Web/Components/` (appears on click without full page navigation)

### Documentation & Reference

- [ ] T110 [P] Generate API documentation (Swagger/OpenAPI) for all three APIs in each service's Program.cs
- [ ] T111 [P] Create README.md files for each service explaining configuration, dependencies, and local dev setup in `src/Taskify.{ServiceName}/README.md`
- [ ] T112 Create frontend setup guide in `src/Taskify.Web/README.md` (how to connect to AppHost services, environment config)
- [ ] T113 [P] Add inline code comments to complex validation and SignalR logic in all services
- [ ] T114 Create TESTING.md guide at repo root (how to run xUnit tests, bUnit tests, integration tests, coverage reporting)

### Performance & Observability

- [ ] T115 [P] Verify OpenTelemetry tracing is active in AppHost and all services for debugging and performance monitoring in `src/Taskify.ServiceDefaults/`
- [ ] T116 [P] Add request logging middleware to all API projects in Program.cs
- [ ] T117 Monitor AppHost dashboard during local dev: verify no excessive error logs, health checks passing, resource metrics reasonable
- [ ] T118 [P] Run performance checks: verify task move, task creation, and comment addition respond in <200ms at client; SignalR broadcasts propagate <1 second (per plan.md goals)

### Accessibility & Browser Compatibility

- [ ] T119 [P] Verify keyboard accessibility: tab through all forms, activate buttons with Enter/Space, dismiss modals with Escape in `src/Taskify.Web/Components/`
- [ ] T120 [P] Test drag-and-drop in Chromium, Firefox, and Safari; verify visual feedback works on touch devices
- [ ] T121 [P] Add ARIA labels and roles to all form controls, buttons, and interactive elements in `src/Taskify.Web/Components/`
- [ ] T122 [P] Run Lighthouse accessibility audit and fix any major issues. Target: **WCAG 2.1 Level AA** compliance (Lighthouse score ≥90). No critical or high-severity accessibility issues permitted. Reference: see `specs/001-taskify-kanban/design-guide.md` for accessibility standards and testing checklist.

### Sample Data & Quickstart Validation

- [ ] T123 Verify quickstart.md validation steps work end-to-end: run AppHost, open web frontend, perform all three user story scenarios within time limits (30s board load, 3m create+assign+move, 1m comment+find). Reference detailed test scenarios in `scripts/usability-test-script.md`; test with at least 2 of the 5 predefined users.
- [ ] T124 [P] Validate seeding: Assert that across all 3 projects, at least 1 task exists in each KanbanColumn (ToDo, InProgress, InReview, Done) and at least 1 task has a comment, verified BEFORE app first run via seed validation script or initial database assertions in `src/Taskify.ProjectsApi/Data/ProjectsSeeder.cs` and `src/Taskify.TasksApi/Data/TasksSeeder.cs`. Reference: `scripts/usability-test-script.md` section "Sample Data Validation (Must Exist)".
- [ ] T125 [P] Test with all 5 predefined users: verify each can log in as any identity without session conflict

### Final Integration Checkpoint

- [ ] T126 [P] Run complete end-to-end quickstart scenario with five users, three projects, sample tasks in all columns, comments visible
- [ ] T127 [P] Run `dotnet test` from repo root; ensure 100% of contract, integration, and component tests pass
- [ ] T128 Verify Aspire AppHost dashboard shows all 4 services healthy and endpoints accessible
- [ ] T129 [P] Perform final manual validation: create new project, add task, assign to user, move through columns, add comment, verify real-time updates on separate browser window

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — **BLOCKS all user stories**
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - User stories can proceed in **parallel** once Foundational is complete
  - Or sequentially in **priority order** (P1 → P2 → P3) if team capacity is limited
- **Polish (Phase 6)**: Depends on all user stories being implemented and testable

### User Story Dependencies

- **User Story 1 (P1) — Track Work**: Can start after Foundational (Phase 2)
  - No dependencies on other stories
  - Enables the core Kanban board experience
  - **MVP scope**: Ship with just US1 if needed; users can view and move existing tasks

- **User Story 2 (P2) — Create & Assign**: Can start after Foundational (Phase 2)
  - No hard dependencies on US1, but benefits from it being complete (easier to test create flow if board already works)
  - Adds project/task creation capability
  - **MVP+1 scope**: Ship US1 + US2 together; users can plan work

- **User Story 3 (P3) — Comments**: Can start after Foundational (Phase 2)
  - No hard dependencies on US1 or US2
  - Adds discussion capability
  - **Full scope**: Ship all three stories; users can discuss and coordinate

### Within Each User Story

1. **Write failing contract tests FIRST** (marked [US?] in test section)
2. **Implement API contracts** (backend services)
3. **Implement notification/event broadcasting** (real-time infrastructure)
4. **Write failing bUnit component tests** (frontend behavior)
5. **Implement frontend components** (Blazor Server)
6. **Run integration tests** to verify cross-service flows
7. **Checkpoint**: Story complete and independently testable

### Parallel Opportunities

#### Within Phase 1 (Setup)
- All T002-T005 tasks marked [P] can run in parallel (different projects, no dependencies)

#### Within Phase 2 (Foundational)
- All T006-T027 tasks marked [P] can run in parallel across service projects
- Service foundation tasks (T011-T027) are independent: ProjectsApi, TasksApi, NotificationsApi, Web can be scaffolded simultaneously
- Shared infrastructure (T006-T010) is lightweight and can complete early to unblock service tasks

#### Within Phase 3-5 (User Stories)
- **Between stories**: All three user stories (US1, US2, US3) can run in **complete parallel** once Foundational (Phase 2) is done
  - Assign different team members/pairs to each story
  - Stories do not block each other; each is independently testable
  
- **Within a story** (e.g., US1):
  - All contract tests marked [P] can run in parallel (different test files)
  - API implementations marked [P] can run in parallel (different service files)
  - Frontend components marked [P] can run in parallel (different .razor files)
  - **Sequential dependency example**: T050 (integrate SignalR) depends on T044-T045 (implement SignalR broadcasting), so T050 must wait for T044-T045

#### Within Phase 6 (Polish)
- All T100-T122 tasks marked [P] can run in parallel (different components, docs, tests)
- Some dependencies exist: T104-T109 (frontend) depend on US1-US3 components existing; document T110-T114 can start anytime

---

## Parallel Execution Example: User Story 1

### Parallel Group A: ProjectsApi Setup & Testing
```
Team Member 1:
  - T030 (write contract tests)
  - T036 (seed sample projects)
  - T037 (GET /api/projects)
  - T038 (GET /api/projects/{id})

Runs tests:
  dotnet test tests/Taskify.ProjectsApi.Tests/
```

### Parallel Group B: TasksApi Setup & Testing
```
Team Member 2:
  - T031 (write contract tests)
  - T032 (write contract tests for move)
  - T040 (seed sample tasks)
  - T039 (GET /api/tasks)
  - T041 (validate column move)
  - T042 (PATCH /api/tasks/{id}/column)
  - T043 (emit TaskMoved event)

Runs tests:
  dotnet test tests/Taskify.TasksApi.Tests/
```

### Parallel Group C: NotificationsApi Setup & Testing
```
Team Member 3:
  - T033 (write integration tests for SignalR)
  - T044 (POST /api/notifications/events)
  - T045 (BoardHub.TaskMoved)
  - T046 (GET /api/notifications/events)

Runs tests:
  dotnet test tests/Taskify.NotificationsApi.Tests/
```

### Parallel Group D: Frontend Components & Testing
```
Team Member 4:
  - T034-T035 (write bUnit component tests)
  - T047-T051 (implement Kanban board component)
  - T048-T049 (implement drag-and-drop)

Runs tests:
  dotnet test tests/Taskify.Web.Tests/
```

### Sync Point: Integration
```
All groups complete their unit/component tests, then:
  - T053: Run full Aspire.Hosting.Testing integration suite
  - Verify end-to-end: create board in frontend → fetch tasks from API → move task → receive SignalR broadcast → update UI
```

---

## MVP Scope & Delivery Strategy

### Minimum Viable Product (MVP) = Phase 1 + Phase 2 + Phase 3 (User Story 1 only)

**Delivers**:
- Three sample projects with seeded tasks across all four Kanban columns
- Blazor Server board UI with drag-and-drop task movement
- Real-time updates via SignalR when any client moves a task
- Five predefined users can select an identity and move tasks
- No login required; no project/task creation yet; no comments yet

**Time Estimate**: ~40-50 tasks (T001-T053) assuming moderate team (2-4 developers)

**Success Criteria** (from spec.md):
- Open any sample project, see all 4 columns with tasks
- Move a task, see it update on other browsers in real time
- Repeat with all 5 users — all succeed
- Task details (title, assignee, existing comments from seed data) survive the move

---

### MVP + Phase 4 (User Story 2)

**Adds**:
- Create new projects with unique names (validation)
- Create tasks within projects with title, description, optional assignee
- Assign/reassign tasks to any of 5 predefined users
- Real-time updates when a project or task is created
- Field-level error messages for validation failures

**Cumulative Time**: ~75-90 tasks (T001-T079)

**Success Criteria**:
- Create a new project, verify it appears in project list and has empty board with 4 columns
- Create a task, assign to a user, see it in To Do with correct assignee
- Try to create with empty name/title — see field error, no item created, other form values preserved
- Real-time notification when another user creates a task on the same board

---

### Full Scope (Phase 1-6: All User Stories + Polish)

**Adds**:
- Comments on tasks with author and creation-time metadata
- Chronological comment ordering
- Real-time comment broadcasting
- Performance optimization, accessibility, docs, final integration tests
- Comprehensive API documentation (Swagger)
- Service READMEs and testing guide

**Cumulative Time**: ~129 tasks (T001-T129)

**Ship**: Production-ready Taskify workspace with all three user story capabilities, well-tested and documented

---

## Task Execution Guidelines

### Before Starting

1. **Foundational Phase MUST complete first** — no user story work until all of Phase 2 (T006-T029) is done
2. **Write failing tests FIRST** — for each story phase, implement tests that fail, then implement the feature to make them pass
3. **Use feature branches** — one branch per user story (e.g., `feature/us1-track-work`) to isolate work and enable parallel PR reviews

### During Implementation

1. **Test-driven**: Ensure tests fail before implementing, pass after
2. **Incremental**: Commit frequently (every task or small group of tasks); push to enable async PR feedback
3. **Parallel teams**: Different teams work on different stories; sync daily on blockers
4. **Real-time validation**: Run `dotnet test` frequently to catch integration issues early

### Code Review Checkpoints

- **After Phase 1**: Verify project structure and dependencies are correct
- **After Phase 2**: Verify all services boot, service discovery works, health checks pass
- **After each User Story Phase**: Verify contract tests pass, integration tests pass, components render correctly
- **After Phase 6**: Full end-to-end validation with quickstart.md scenario

---

## Format Validation Checklist

✅ **All tasks follow strict checklist format**:
- [ ] T### Task ID (sequential)
- [ ] [P?] Parallelizable marker (if applicable)
- [ ] [US?] Story label (if user story phase)
- [ ] Clear action verb and exact file path
- [ ] No task missing any required component

✅ **Phases are logically ordered**: Setup → Foundational → User Stories (P1, P2, P3) → Polish

✅ **Each story is independently testable**: Contract tests + implementation + component tests + integration validation

✅ **Parallel execution is clearly marked**: [P] indicates parallelizable tasks within and across phases

✅ **Dependencies are documented**: Phase dependencies, story dependencies, within-story sequential ordering explained

✅ **MVP scope is clear**: US1 alone is shiable; US1+US2 recommended; full scope is US1+US2+US3+Polish
