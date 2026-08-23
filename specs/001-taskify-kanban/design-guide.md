# Design Guide: Taskify Kanban Workspace

**Purpose**: Define visual design, component specifications, accessibility standards, and responsive layout guidelines for the Taskify web application.

**Target Audience**: Frontend developers, UX designers, QA testers, accessibility auditors.

---

## Color Palette

### Primary Colors
- **Brand Purple**: `#6D28D9` — Used for primary action buttons, links, active states
- **Neutral Gray**: `#F5F7FA` — Background for secondary surfaces, task cards
- **Dark Gray**: `#1F2937` — Text color, component borders

### Semantic Colors
- **Success Green**: `#10B981` — Positive actions, success messages (e.g., "Comment added")
- **Warning Orange**: `#F59E0B` — Caution messages, pending states (e.g., "Saving...")
- **Error Red**: `#EF4444` — Validation errors, destructive confirmations (e.g., "Invalid input")
- **Info Teal**: `#0F766E` — Informational messages (e.g., "Task moved by Jordan")

### Contrast Requirements (WCAG 2.1 Level AA)
- Text on background: Minimum ratio 4.5:1 for normal text, 3:1 for large text (≥18pt or ≥14pt bold)
- Interactive components: Minimum ratio 3:1 for button backgrounds and borders
- Verify all color combinations using WebAIM Contrast Checker before release

### Examples
```
✓ Dark Gray (#1F2937) text on Neutral Gray (#F5F7FA) background: 11.5:1 contrast ✅
✓ Brand Purple (#6D28D9) button on Neutral Gray background: passes AA contrast ✅
✓ Brand Purple (#6D28D9) text on Neutral Gray background: passes AA contrast ✅
```

---

## Responsive Layout

### Breakpoints (Mobile-First)

| Device | Breakpoint | Width | Layout |
|--------|-----------|-------|--------|
| Mobile | `sm` | 640px | Single column; vertical task stack |
| Tablet | `md` | 768px | 2-column board; scrollable columns |
| Desktop | `lg` | 1024px | 4-column board (full width) |
| Wide Desktop | `xl` | 1280px | 4-column board with sidebar |

### Layout Rules

- **Mobile (< 640px)**: Kanban board displays one column at a time; users swipe horizontally or use column selector dropdown to navigate between ToDo, InProgress, InReview, Done
- **Tablet (640px - 1023px)**: Display 2 columns side-by-side; scrollable horizontally if needed
- **Desktop (≥ 1024px)**: Display all 4 columns in a single viewport; no horizontal scrolling required
- **Spacing**: Maintain consistent 16px margin/padding between elements (baseline unit)
- **Max Content Width**: Limit board container to 1400px on extra-wide screens to prevent visual sprawl

### Example CSS Grid
```css
.kanban-board {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 16px;
  padding: 16px;
}

@media (max-width: 768px) {
  .kanban-board {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .kanban-board {
    grid-template-columns: 1fr;
    overflow-x: auto;
  }
}
```

---

## Typography

### Font Family
- **Primary**: System stack: `-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Oxygen, Ubuntu, Cantarell, sans-serif`
- **Monospace** (for technical content): `"SF Mono", Monaco, "Cascadia Code", Roboto Mono, Consolas, monospace`

### Type Scale

| Role | Font Size | Font Weight | Line Height | Usage |
|------|-----------|-------------|-------------|-------|
| **Display** | 32px | 700 (Bold) | 1.2 | Page title, main heading |
| **Heading 1** | 24px | 600 (Semibold) | 1.3 | Section headers |
| **Heading 2** | 20px | 600 (Semibold) | 1.3 | Subsection headers |
| **Body Large** | 16px | 400 (Regular) | 1.5 | Primary body text, large labels |
| **Body** | 14px | 400 (Regular) | 1.5 | Default text, form labels, comments |
| **Small** | 12px | 400 (Regular) | 1.4 | Helper text, timestamps, metadata |
| **Caption** | 11px | 400 (Regular) | 1.4 | Watermarks, very small labels |

### Line Length
- Maximum 70 characters per line for body text to aid readability
- Apply max-width constraints: `max-width: 65ch` to prose containers

---

## Component Specifications

### Kanban Column Container

```
Dimensions:
  - Width: 100% on mobile, 280px on desktop
  - Min-height: 400px (empty state visible)
  - Border-radius: 8px
  - Border: 1px solid #E5E7EB (light gray)
  - Background: #F9FAFB (very light gray)

Spacing:
  - Padding: 12px
  - Gap between cards: 8px

Header:
  - Column name (e.g., "ToDo"): 14px semibold, Dark Gray
  - Task count badge: 12px, Brand Purple background, white text, border-radius 12px
  - Min-height: 40px
  - Border-bottom: 2px solid Brand Purple

States:
  - Empty: Display placeholder text "No tasks yet" in 12px light gray
  - Hover (drag-over): Background becomes 95% opacity; drop zone indicator appears
```

### Task Card

```
Dimensions:
  - Width: 100% (fills column)
  - Min-height: 60px
  - Border-radius: 6px
  - Border: 1px solid #D1D5DB (medium gray)
  - Background: White

Spacing:
  - Padding: 12px
  - Gap between elements: 8px

Layout:
  - Title: 14px semibold, Dark Gray, single line with text-overflow ellipsis
  - Assignee: 12px regular, Brand Purple, display user avatar (if available) or initials
  - Description: 12px regular, medium gray, 2-line max with ellipsis (if present)
  - Comment count badge (if > 0): 11px, Success Green background, white text, bottom-right corner

States:
  - Normal: White background, medium border
  - Hover: Box shadow 0 4px 6px rgba(0, 0, 0, 0.1); cursor grab
  - Dragging: Opacity 0.7, box shadow 0 10px 15px rgba(0, 0, 0, 0.2); cursor grabbing
  - Drop-Target: Background becomes 90% opacity with dashed border
  - Active (clicked/selected): Border becomes 2px solid Brand Purple
```

### Form Input Fields

```
Dimensions:
  - Height: 40px (default)
  - Border-radius: 6px
  - Border: 1px solid #D1D5DB
  - Padding: 10px 12px

States:
  - Default: White background, medium gray border
  - Focus: White background, 2px Brand Purple border, box shadow 0 0 0 3px rgba(109, 40, 217, 0.1)
  - Filled: White background, Dark Gray text
  - Error: White background, 2px Error Red border, red label text
  - Disabled: #F3F4F6 background, #9CA3AF text, opacity 0.5

Label:
  - Font: 14px semibold, Dark Gray
  - Margin-bottom: 6px
  - Display: block
  - Required indicator: Red asterisk (*) after label text

Error Message:
  - Font: 12px regular, Error Red
  - Margin-top: 4px
  - Display: Inline under the input field
  - Icon: Small error icon (⚠️) before text

Helper Text:
  - Font: 12px regular, medium gray
  - Margin-top: 4px
  - Display: Inline under the input field or label
  - Max-width: 100% of input width
```

### Buttons

```
Primary Button (Action Buttons):
  - Background: Brand Purple (#6D28D9)
  - Text: White, 14px semibold
  - Padding: 10px 16px
  - Border-radius: 6px
  - Border: None
  - Min-width: 100px
  - Hover: Background darken to #0052A3
  - Active: Background darken to #003D7A
  - Disabled: Background #D1D5DB, text #9CA3AF, opacity 0.5, cursor not-allowed
  - Focus: Add outline with Focus Ring (see below)

Secondary Button (Less Critical Actions):
  - Background: Neutral Gray (#F5F7FA)
  - Text: Dark Gray, 14px semibold
  - Padding: 10px 16px
  - Border-radius: 6px
  - Border: 1px solid #D1D5DB
  - Hover: Background #E5E7EB
  - Active: Background #D1D5DB
  - Focus: Add outline with Focus Ring

Destructive Button (Delete/Confirm Danger):
  - Background: Error Red (#EF4444)
  - Text: White, 14px semibold
  - Same padding/border-radius as Primary
  - Hover: Background darken to #DC2626
  - Requires confirmation dialog before action

Focus Ring (for Keyboard Navigation):
  - Outline: 2px solid Brand Purple
  - Outline-offset: 2px
  - Border-radius: 4px
```

### Modals & Dialogs

```
Overlay:
  - Background: Black with 50% opacity (rgba(0, 0, 0, 0.5))
  - Covers entire viewport
  - Z-index: 1000 (above all other content)

Modal Container:
  - Background: White
  - Border-radius: 8px
  - Box shadow: 0 20px 25px rgba(0, 0, 0, 0.15)
  - Padding: 24px
  - Max-width: 600px on mobile, 500px on desktop
  - Min-width: 320px

Header:
  - Title: 20px semibold, Dark Gray
  - Close button: 16px icon (×), positioned top-right, hover background #F3F4F6
  - Margin-bottom: 16px

Body:
  - Font: 14px regular, Dark Gray
  - Line-height: 1.5
  - Margin-bottom: 24px

Footer:
  - Display: flex, justify-content: flex-end, gap: 12px
  - Contain form buttons (Submit, Cancel, etc.)

Keyboard Handling:
  - ESC key closes modal
  - Tab focus cycles within modal only (trap focus)
  - Enter key submits form (if applicable)
```

### Comments Section

```
Comment Container:
  - Border: 1px solid #E5E7EB
  - Border-radius: 6px
  - Padding: 12px
  - Background: #F9FAFB
  - Margin-bottom: 8px

Timestamp:
  - Font: 11px regular, medium gray
  - Display: right-aligned or inline

Author:
  - Font: 12px semibold, Dark Gray
  - Avatar: 24px circle, initials or image
  - Display inline with timestamp

Comment Text:
  - Font: 14px regular, Dark Gray
  - Line-height: 1.5
  - Word-wrap: break-word
  - Max-width: 100% of container

Chronological Ordering:
  - Oldest comment at top
  - Newest comment at bottom
  - Separator line (1px light gray) between comments
```

---

## Accessibility Standards (WCAG 2.1 Level AA)

### Required Implementations

| Requirement | Implementation |
|-------------|-----------------|
| **Keyboard Navigation** | All interactive elements (buttons, links, form inputs, drag-drop) must be accessible via Tab/Shift-Tab and Enter/Space keys. No element should require a mouse. |
| **Focus Indicators** | All focusable elements must have a visible focus ring (minimum 2px, 3:1 contrast ratio). Focus must never be invisible or hidden. |
| **ARIA Labels** | All form inputs must have associated `<label>` elements or `aria-label` attributes. Buttons must have descriptive text or `aria-label`. Icons must have `aria-label` or `title` attribute. |
| **Color Contrast** | All text (including form labels, buttons, links) must have minimum 4.5:1 contrast ratio. Large text (≥18pt) may use 3:1. Logos and decorative elements may be exempt. |
| **Alt Text** | All images, icons, and avatars must have descriptive alt text or `aria-label`. User avatars may use user initials as alt text (e.g., "JL for Jordan Lee"). |
| **Form Validation** | Error messages must be associated with form fields using `aria-describedby`. Error messages must be announced to screen readers. |
| **Headings** | Use semantic heading hierarchy (h1, h2, h3). Do not skip heading levels. Each page must have exactly one `<h1>`. |
| **Links** | All links must have descriptive link text (e.g., "Open Project A" not "Click here"). Avoid link text like "Read more" without context. |
| **Skip Navigation** | Provide a "Skip to main content" link at the top of the page to bypass repetitive navigation. |
| **Mobile Touch Targets** | All interactive elements must have a minimum 44×44px touch target size to accommodate users with motor impairments. |
| **Screen Reader Testing** | Test with NVDA (Windows), JAWS (Windows), or VoiceOver (Mac/iOS) to ensure all content is readable and interactive. |
| **Reduced Motion** | Respect `prefers-reduced-motion` media query; disable animations/transitions for users who have motion sensitivity. |

### Testing Checklist

Before release, perform:
1. ✅ Run automated accessibility audit: Lighthouse Accessibility or Axe DevTools (target: 90+/100)
2. ✅ Manual keyboard navigation: Tab through entire app; verify all buttons/links/inputs are reachable
3. ✅ Screen reader test: Use VoiceOver (Mac) or NVDA (Windows); verify page structure and form labels are announced correctly
4. ✅ Color contrast check: Use WebAIM Contrast Checker for all text/background pairs
5. ✅ Touch target test: Verify all buttons/clickable elements are ≥44×44px
6. ✅ Mobile responsiveness: Test on devices ranging from 320px width (iPhone SE) to 2560px width (wide desktop)

---

## Performance Targets

| Metric | Target | Tool |
|--------|--------|------|
| **Board Interaction Response Time** | ≤ 200ms (task move, assignment, comment) | Browser DevTools Performance tab |
| **SignalR Real-Time Propagation** | ≤ 1 second (board updates visible on connected clients) | Network tab in DevTools |
| **Page Load (First Contentful Paint)** | ≤ 2 seconds | Lighthouse |
| **Largest Contentful Paint (LCP)** | ≤ 2.5 seconds | Lighthouse |
| **Cumulative Layout Shift (CLS)** | ≤ 0.1 | Lighthouse |

---

## Visual Refinements

### Drop Zone Indicator (Drag & Drop)
- Display a **dashed border** (2px, Brand Purple) around the target column when user drags a task over it
- Add a **highlight color** (Brand Purple at 10% opacity) to the column background
- Show a **visual placeholder** (thin gray box, 48px height) indicating where the task will be dropped
- Remove indicator immediately when drag ends

### Loading States
- Display a **spinner icon** (CSS animation or animated SVG) centered in the component
- Add a **loading text**: "Saving...", "Creating...", or "Loading..."
- Disable user interaction (opacity 0.6) while loading
- Timeout: Show error message if loading takes >5 seconds

### Empty States
- Display a **centered, friendly message** with an icon:
  - "No projects yet. Create your first project to get started." (project list)
  - "No tasks in this column." (empty Kanban column)
  - "No comments yet. Start the conversation!" (task with no comments)
- Add a **CTA button** (Create Project / Add Task / Add Comment) below the message

### Success/Error Messages
- **Toast notifications**: Small, fixed-position alerts (top-right, 300px width)
  - Success: Green background, Success Green text, white icon (✓)
  - Error: Red background, white text, red icon (✗)
  - Auto-dismiss after 4 seconds
  - Allow manual dismiss with close button

---

## Browser Support

- **Chrome/Edge**: Latest 2 versions
- **Firefox**: Latest 2 versions
- **Safari**: Latest 2 versions (macOS 12+, iOS 15+)
- **Mobile Safari (iOS)**: iOS 14+
- **Chrome Android**: Latest 2 versions

**Note**: Do not support Internet Explorer 11 or below. Target modern browsers with ES6+ support and CSS Grid/Flexbox.

---

## Example Component Layouts

### Kanban Board (Desktop)
```
┌─────────────────────────────────────────────────────────────┐
│ Taskify | Project: Q4 Roadmap  |  User: Jordan Lee  [↓]     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐  ┌─────────┐         │
│  │ ToDo    │  │Progress │  │ Review  │  │ Done    │         │
│  │ (3)     │  │ (2)     │  │ (1)     │  │ (2)     │         │
│  ├─────────┤  ├─────────┤  ├─────────┤  ├─────────┤         │
│  │         │  │         │  │         │  │         │         │
│  │┌─────┐  │  │┌─────┐  │  │┌─────┐  │  │┌─────┐  │         │
│  ││ Design│  ││Build  │  ││Review  ││  ││Deploy ││         │
│  ││  UI  │  ││Modal  │  ││Feedback││  ││to  Prod││         │
│  ││ JL  │  ││ PS   │  ││ LG    ││  ││ MC   ││         │
│  ││ 💬 1 │  ││      │  ││💬  1 ││  ││      ││         │
│  │└─────┘  │  │└─────┘  │  │└─────┘  │  │└─────┘  │         │
│  │         │  │         │  │         │  │         │         │
│  │┌─────┐  │  │┌─────┐  │  │         │  │┌─────┐  │         │
│  ││ Write│  ││ Fix   │  │  │         │  ││ Admin│  │         │
│  ││ Docs │  ││ Bugs  │  │  │         │  ││ Panel  ││         │
│  ││ (no) │  ││ JL   │  │  │         │  ││ (no)  ││         │
│  │└─────┘  │  │└─────┘  │  │         │  │└─────┘  │         │
│  │         │  │         │  │         │  │         │         │
│  │┌─────┐  │  │         │  │         │  │         │         │
│  ││ Setup│  │  │         │  │         │  │         │         │
│  ││ Repo │  │  │         │  │         │  │         │         │
│  ││ MC  │  │  │         │  │         │  │         │         │
│  │└─────┘  │  │         │  │         │  │         │         │
│  │         │  │         │  │         │  │         │         │
│  └─────────┘  └─────────┘  └─────────┘  └─────────┘         │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Create Task Form
```
┌─────────────────────────────────┐
│ Create Task                   × │
├─────────────────────────────────┤
│                                 │
│ Project: Q4 Roadmap            │
│ (read-only)                     │
│                                 │
│ Task Title *                    │
│ ┌────────────────────────────┐ │
│ │ [Type task title...        ]│ │
│ └────────────────────────────┘ │
│                                 │
│ Description                     │
│ ┌────────────────────────────┐ │
│ │ [Type description...   ]   │ │
│ │                            │ │
│ └────────────────────────────┘ │
│                                 │
│ Assign To                       │
│ ┌──────────────────────────────┐│
│ │ ▼ Select a user...           ││
│ │ ☐ Jordan Lee (Engineer)      ││
│ │ ☐ Priya Shah (Engineer)      ││
│ │ ☐ Luis Garcia (Engineer)     ││
│ │ ☐ Ava Williams (Engineer)    ││
│ │ ☐ Maya Chen (Product Manager)││
│ └──────────────────────────────┘│
│                                 │
│            [Cancel]  [Create]  │
│                                 │
└─────────────────────────────────┘
```

---

## Design Review Checklist

Before committing frontend changes, verify:
- [ ] All text meets 4.5:1 contrast ratio (WebAIM checker)
- [ ] All interactive elements are ≥44×44px touch targets
- [ ] Keyboard Tab order is logical (left-to-right, top-to-bottom)
- [ ] Focus ring is visible on all focusable elements
- [ ] Form labels are associated with inputs (`<label for="...">`)
- [ ] Error messages are clear and actionable
- [ ] Loading states are visually distinct
- [ ] Empty states are friendly and include CTAs
- [ ] Responsive layout works on 320px, 768px, and 1920px widths
- [ ] Color palette is consistent (use hex codes, not RGB)
- [ ] Typeface hierarchy matches type scale (no arbitrary sizes)
- [ ] Spacing is multiples of 4px or 8px baseline unit
- [ ] Icons are labeled with alt text or aria-label
- [ ] No content is communicated by color alone (use labels/patterns)
- [ ] Animations respect prefers-reduced-motion
