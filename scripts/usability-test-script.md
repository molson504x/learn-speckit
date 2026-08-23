# Usability Test Script: Taskify Kanban Workspace

**Purpose**: Validate success criteria SC-001, SC-002, and SC-003 through scripted usability validation with five representative team members.

**Prerequisites**: Clean seeded workspace with 3 sample projects, 4 workflow columns per project (ToDo, InProgress, InReview, Done), and predefined user roster (Maya Chen PM, Jordan Lee Engineer, Priya Shah Engineer, Luis Garcia Engineer, Ava Williams Engineer).

**Test Environment Setup**:
- Start with a freshly seeded database (delete any prior test data)
- Use a stopwatch or timer for all time measurements
- Test on a laptop/desktop with standard browser (Chrome, Firefox, or Safari)
- Ensure internet connectivity for any cloud-hosted instance

---

## Scenario 1: Board Load & Task Discovery (SC-001)

**Time Limit**: 30 seconds  
**Success Threshold**: At least 4 of 5 users  
**Measurable Outcome**: User can open any of the three sample project boards and identify every task's workflow column and assignee.

### Test Steps

1. **Start Timer**
2. Open the Taskify application in your browser
3. You see a list of three sample projects: "Project A", "Project B", "Project C" (or equivalent implementation names)
4. Click to open **any one** of the three projects (suggest starting with Project A)
5. The project's Kanban board loads with four columns visible: **ToDo**, **InProgress**, **InReview**, **Done**
6. **For each task visible on the board**, identify and speak aloud:
   - The task title
   - Which column it is in
   - Who it is assigned to (if anyone)
7. **Stop Timer**

### Acceptance Criteria

- ✅ All tasks visible within the board are correctly identified with their column and assignee
- ✅ User completes all identifications within 30 seconds
- ✅ No errors, timeouts, or reload required

### Sample Data Validation (Must Exist)

Ensure sample data includes:
- At least one task in each of the four columns (ToDo, InProgress, InReview, Done) *across all three projects*
- At least one task with a visible assignee
- At least one task with no assignee (null)

---

## Scenario 2: Create, Assign, and Move Task (SC-002)

**Time Limit**: 3 minutes  
**Success Threshold**: At least 4 of 5 users on their **first attempt**  
**Measurable Outcome**: User can create a project, add and assign a task, and move it to InProgress.

### Test Steps

1. **Start Timer**
2. Return to the project list (if not already there)
3. **Create a new project**:
   - Click "Create Project" or equivalent button
   - Enter a project name (e.g., "My Test Project")
   - Submit the form
   - Confirm the new project appears in the list and can be opened
4. **Open the newly created project board**
5. **Create a new task**:
   - Click "Create Task" or equivalent button in the ToDo column
   - Enter a task title (e.g., "Write test documentation")
   - Enter an optional description (e.g., "Add detailed test scenarios")
   - Select an assignee from the predefined roster (e.g., "Jordan Lee")
   - Submit the form
   - Confirm the task appears in the **ToDo column** with the selected assignee
6. **Move the task to InProgress**:
   - Drag the task from ToDo to InProgress column (or use equivalent move UI)
   - Confirm the task appears in InProgress and retains its title and assignee
7. **Stop Timer**

### Acceptance Criteria

- ✅ New project is created and visible in the project list
- ✅ New project's board loads successfully with four empty columns
- ✅ New task is created in ToDo column with title, description, and assignee visible
- ✅ Task successfully moves to InProgress and retains all details
- ✅ User completes all steps within 3 minutes on first attempt
- ✅ No validation errors or partial form states encountered

### Test Variations (Run with Different Users)

- Test 1: Assign to Maya Chen (Product Manager)
- Test 2: Assign to Jordan Lee (Engineer)
- Test 3: Assign to Priya Shah (Engineer)
- Test 4: Assign to Luis Garcia (Engineer)
- Test 5: Assign to Ava Williams (Engineer)

---

## Scenario 3: Add and Find Comment (SC-003)

**Time Limit**: 1 minute  
**Success Threshold**: At least 4 of 5 users on their **first attempt**  
**Measurable Outcome**: User can add a non-empty comment to an existing task and find it again.

### Test Steps

1. **Start Timer**
2. Open any of the three sample projects
3. Click on any task card to open its details (or open the task detail view)
4. Locate the **Comments section** (usually at the bottom of the task details)
5. **Add a comment**:
   - Click "Add Comment" or equivalent input field
   - Type a comment (e.g., "This task is ready for review by the team")
   - Select comment author from predefined roster (or auto-selected as current user)
   - Submit the comment
   - Confirm the comment appears with the author name and timestamp
6. **Close the task detail view** (back to board or task list)
7. **Reopen the same task** to verify the comment persists
8. Confirm the comment is visible, includes the author name and creation time, and is sorted chronologically with any other comments
9. **Stop Timer**

### Acceptance Criteria

- ✅ Comment form is accessible and intuitive
- ✅ Comment is successfully submitted and displays author + timestamp
- ✅ Comment persists when task is reopened
- ✅ If multiple comments exist, they are displayed in chronological order (oldest first)
- ✅ User completes all steps within 1 minute on first attempt
- ✅ No validation errors or reload required

### Edge Case Validation

- Test **empty comment rejection**: Attempt to submit a blank comment; system should reject it with a clear error message
- Test **whitespace-only rejection**: Attempt to submit comment with only spaces; system should reject it

---

## Scenario 4: Task Integrity After Move (SC-004)

**Time Limit**: 5 minutes  
**Success Threshold**: 100% task integrity  
**Measurable Outcome**: All moved tasks retain their project, title, assignee, and existing comments after being moved across all four workflow columns.

### Test Steps

1. Open any of the three sample projects
2. Select any task that has an assignee and at least one comment (verify these details before moving)
3. **Record baseline state**:
   - Note the task title
   - Note the assignee
   - Note the column it's currently in
   - Note the number and content of comments
4. **Move the task through all four columns**:
   - Move from current column → ToDo (if not already)
   - Move from ToDo → InProgress
   - Move from InProgress → InReview
   - Move from InReview → Done
5. **After each move**, verify:
   - ✅ Task title is unchanged
   - ✅ Assignee is unchanged
   - ✅ All comments are still present
   - ✅ Task appears in exactly one column
6. **Open the final task detail** (in Done column) and confirm:
   - ✅ Full task information matches the baseline state
   - ✅ Comments are sorted chronologically and unchanged

### Acceptance Criteria

- ✅ 100% of task attributes (title, assignee, comments, project) are preserved across all column moves
- ✅ No data loss or truncation occurs
- ✅ Task appears in only one column at any given time

---

## Recording Results

### Per-User Result Format

```
User: [Name / ID]
Test Environment: [Browser / OS]
Start Time: [HH:MM]

Scenario 1 (Board Load):
  Status: [PASS / FAIL]
  Time: [seconds] / 30s limit
  Notes: [Any issues or observations]

Scenario 2 (Create & Assign):
  Status: [PASS / FAIL]
  Time: [seconds] / 3m limit
  First Attempt: [YES / NO]
  Notes: [Any validation errors, form issues, etc.]

Scenario 3 (Comment):
  Status: [PASS / FAIL]
  Time: [seconds] / 1m limit
  First Attempt: [YES / NO]
  Notes: [Comment persistence, display issues, etc.]

Scenario 4 (Task Integrity):
  Status: [PASS / FAIL]
  Columns Tested: [2 / 3 / 4]
  Notes: [Any data loss, display issues, etc.]

Overall: [PASS / FAIL]
```

### Aggregated Results

```
Scenario 1 (Board Load - 30s):
  Passed: [4-5] / 5
  Status: [PASS if 4+ passed, FAIL otherwise]

Scenario 2 (Create & Assign - 3m):
  Passed: [4-5] / 5 (on first attempt)
  Status: [PASS if 4+ passed, FAIL otherwise]

Scenario 3 (Comment - 1m):
  Passed: [4-5] / 5 (on first attempt)
  Status: [PASS if 4+ passed, FAIL otherwise]

Scenario 4 (Task Integrity - 100%):
  Passed: [YES / NO]
  Status: [PASS if 100% integrity confirmed, FAIL otherwise]
```

---

## Troubleshooting

| Issue | Resolution |
|-------|-----------|
| Browser doesn't load app | Check network connectivity; clear browser cache; try incognito mode |
| Tasks not visible in columns | Verify sample data was seeded correctly; check for empty project state |
| Drag-and-drop not working | Verify browser supports drag-and-drop; try alternative move UI if available |
| Comment not saving | Check for validation errors; verify author selection; try reloading task detail |
| Timer runs out | Record FAIL and note the step where user got stuck; consider UI/workflow issues |

---

## Notes for Test Facilitator

- Provide minimal instruction; let users explore the UI naturally
- Do not coach users to find features; if they cannot find a feature within the time limit, record FAIL
- Ensure a quiet test environment with no interruptions
- Document any UI confusion or confusing error messages for post-test debrief
- Collect feedback on what was intuitive vs. what was unclear
- Use results to identify areas for UI refinement before final release
