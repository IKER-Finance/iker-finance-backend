# Feedback Feature - Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend (React/Vue/Angular)             │
│                                                                  │
│  ┌──────────────────┐              ┌─────────────────────────┐ │
│  │  User Interface  │              │  Admin Dashboard        │ │
│  │                  │              │                         │ │
│  │ - Feedback Form  │              │ - Feedback List         │ │
│  │ - Submit Button  │              │ - Status Updates        │ │
│  │                  │              │ - Search & Filter       │ │
│  └──────────────────┘              └─────────────────────────┘ │
│           │                                     │                │
│           │  JWT Token                          │ JWT Token     │
│           │  (User Role)                        │ (Admin Role)  │
└───────────┼─────────────────────────────────────┼───────────────┘
            │                                     │
            ▼                                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                         API Layer (ASP.NET Core)                 │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              FeedbackController                           │  │
│  │                                                           │  │
│  │  POST   /api/feedback              [Authorize]           │  │
│  │  GET    /api/feedback              [Authorize(Admin)]    │  │
│  │  PATCH  /api/feedback/{id}/status  [Authorize(Admin)]    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    MediatR (CQRS)                         │  │
│  │                                                           │  │
│  │  Commands:                    Queries:                   │  │
│  │  - CreateFeedbackCommand      - GetFeedbacksQuery        │  │
│  │  - UpdateFeedbackStatusCmd                               │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            Application Services & Handlers                │  │
│  │                                                           │  │
│  │  - CreateFeedbackCommandHandler                          │  │
│  │  - GetFeedbacksQueryHandler                              │  │
│  │  - UpdateFeedbackStatusCommandHandler                    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                 FeedbackRepository                        │  │
│  │                                                           │  │
│  │  - GetFeedbacksAsync() with filtering & pagination       │  │
│  └──────────────────────────────────────────────────────────┘  │
└───────────────────────────────┬──────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Database (PostgreSQL)                         │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Feedbacks Table                                          │  │
│  │                                                           │  │
│  │  - Id (PK)                    - Status                   │  │
│  │  - UserId (FK)                - AdminResponse            │  │
│  │  - Type                       - RespondedByUserId (FK)   │  │
│  │  - Subject                    - ResponseDate             │  │
│  │  - Description                - CreatedAt                │  │
│  │  - Priority                   - UpdatedAt                │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  AspNetRoles Table                                        │  │
│  │                                                           │  │
│  │  - Admin                                                 │  │
│  │  - User                                                  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## User Flow Diagrams

### User Submitting Feedback

```
┌─────────┐
│  User   │
└────┬────┘
     │
     │ 1. Clicks "Submit Feedback"
     ▼
┌─────────────────────┐
│  Feedback Form UI   │
│                     │
│  - Type: [Bug]      │
│  - Priority: [High] │
│  - Subject: ...     │
│  - Description: ... │
└────┬────────────────┘
     │
     │ 2. Fills form and clicks Submit
     ▼
┌─────────────────────┐
│  Frontend Validates │
│  - Required fields  │
│  - Max lengths      │
└────┬────────────────┘
     │
     │ 3. POST /api/feedback
     │    Authorization: Bearer {token}
     ▼
┌─────────────────────┐
│  API Validates      │
│  - Token (JWT)      │
│  - Request body     │
└────┬────────────────┘
     │
     │ 4. Creates feedback
     ▼
┌─────────────────────┐
│  Database           │
│  INSERT feedback    │
└────┬────────────────┘
     │
     │ 5. Returns created feedback
     ▼
┌─────────────────────┐
│  Success Message    │
│  "Feedback sent!"   │
└─────────────────────┘
```

---

### Admin Managing Feedback

```
┌─────────┐
│  Admin  │
└────┬────┘
     │
     │ 1. Opens Feedback Dashboard
     ▼
┌─────────────────────┐
│  GET /api/feedback  │
│  + filters, page    │
└────┬────────────────┘
     │
     │ 2. Returns paginated list
     ▼
┌─────────────────────────────────┐
│  Feedback List UI               │
│                                 │
│  [Search] [Status▼] [Type▼]    │
│                                 │
│  ID  User      Subject   Status │
│  1   John Doe  Bug xyz   Open   │
│  2   Jane S.   Feature   Open   │
└────┬────────────────────────────┘
     │
     │ 3. Clicks on feedback
     ▼
┌─────────────────────────────────┐
│  Feedback Detail Modal          │
│                                 │
│  Subject: Bug xyz               │
│  Description: ...               │
│                                 │
│  Status: [InProgress ▼]         │
│  Response: [text area]          │
│                                 │
│  [Update Status]                │
└────┬────────────────────────────┘
     │
     │ 4. Changes status & adds response
     │    PATCH /api/feedback/1/status
     ▼
┌─────────────────────┐
│  Database           │
│  UPDATE feedback    │
│  SET status, resp.  │
└────┬────────────────┘
     │
     │ 5. Returns updated feedback
     ▼
┌─────────────────────┐
│  Success Toast      │
│  "Status updated!"  │
└─────────────────────┘
```

---

## Authentication Flow

```
┌──────────────┐
│  User Login  │
│  or Register │
└──────┬───────┘
       │
       │ POST /api/auth/login or /api/auth/register
       ▼
┌──────────────────────┐
│  Authentication      │
│  Service             │
│                      │
│  1. Validate creds   │
│  2. Fetch user roles │
│  3. Generate JWT     │
└──────┬───────────────┘
       │
       │ JWT Token Generated
       │
       ▼
┌────────────────────────────────────────┐
│  JWT Token Structure                   │
│                                        │
│  Header:                               │
│    { "alg": "HS256", "typ": "JWT" }    │
│                                        │
│  Payload:                              │
│    {                                   │
│      "sub": "user-id",                 │
│      "email": "user@example.com",      │
│      "role": "User" or "Admin",        │
│      "exp": 1234567890                 │
│    }                                   │
│                                        │
│  Signature: [encrypted]                │
└────────┬───────────────────────────────┘
         │
         │ Returned to Frontend
         ▼
┌────────────────────────┐
│  Frontend Stores Token │
│  - localStorage        │
│  - Decodes JWT         │
│  - Extracts role       │
└────────┬───────────────┘
         │
         │ Includes in every request
         ▼
┌────────────────────────┐
│  API Request           │
│  Authorization: Bearer │
│  {token}               │
└────────────────────────┘
```

---

## Data Flow

### Create Feedback (POST)

```
Frontend                  Backend                     Database
   │                         │                            │
   │  POST /api/feedback     │                            │
   ├────────────────────────>│                            │
   │  {                      │                            │
   │    type: 1,             │  Validate JWT              │
   │    subject: "...",      │  Extract UserId            │
   │    description: "..."   │                            │
   │  }                      │                            │
   │                         │                            │
   │                         │  Validate Request          │
   │                         │  (FluentValidation)        │
   │                         │                            │
   │                         │  Create Feedback Entity    │
   │                         │  - Set UserId              │
   │                         │  - Set Status = Open       │
   │                         │  - Set CreatedAt/UpdatedAt │
   │                         │                            │
   │                         │  Save to Database          │
   │                         ├───────────────────────────>│
   │                         │                            │
   │                         │                  INSERT    │
   │                         │                  feedback  │
   │                         │                            │
   │                         │<───────────────────────────┤
   │                         │  Feedback saved (with ID)  │
   │                         │                            │
   │                         │  Build Response DTO        │
   │                         │  - Include user details    │
   │                         │                            │
   │<────────────────────────┤                            │
   │  201 Created            │                            │
   │  { id: 1, ... }         │                            │
   │                         │                            │
```

### Get Feedbacks (GET - Admin)

```
Frontend                  Backend                     Database
   │                         │                            │
   │  GET /api/feedback?     │                            │
   │  status=1&page=1        │                            │
   ├────────────────────────>│                            │
   │  Authorization: Bearer  │                            │
   │                         │                            │
   │                         │  Validate JWT              │
   │                         │  Check Role = "Admin"      │
   │                         │                            │
   │                         │  Parse Query Params        │
   │                         │  - Build filters           │
   │                         │  - Validate pagination     │
   │                         │                            │
   │                         │  Query Database            │
   │                         ├───────────────────────────>│
   │                         │                            │
   │                         │  SELECT with filters       │
   │                         │  JOIN Users                │
   │                         │  ORDER BY, LIMIT, OFFSET   │
   │                         │                            │
   │                         │<───────────────────────────┤
   │                         │  Feedback rows + count     │
   │                         │                            │
   │                         │  Build Paginated Response  │
   │                         │  - Map to DTOs             │
   │                         │  - Calculate total pages   │
   │                         │                            │
   │<────────────────────────┤                            │
   │  200 OK                 │                            │
   │  {                      │                            │
   │    data: [...],         │                            │
   │    totalCount: 45,      │                            │
   │    pageNumber: 1,       │                            │
   │    totalPages: 5        │                            │
   │  }                      │                            │
```

---

## Database Schema

```sql
-- Feedbacks Table
CREATE TABLE "Feedbacks" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "Type" INTEGER NOT NULL,
    "Subject" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(2000) NOT NULL,
    "Priority" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "AdminResponse" VARCHAR(2000),
    "RespondedByUserId" VARCHAR(450),
    "ResponseDate" TIMESTAMP,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL,

    CONSTRAINT "FK_Feedbacks_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES "AspNetUsers"("Id")
        ON DELETE RESTRICT,

    CONSTRAINT "FK_Feedbacks_Users_RespondedByUserId"
        FOREIGN KEY ("RespondedByUserId")
        REFERENCES "AspNetUsers"("Id")
        ON DELETE RESTRICT
);

-- Indexes for performance
CREATE INDEX "IX_Feedbacks_UserId" ON "Feedbacks"("UserId");
CREATE INDEX "IX_Feedbacks_Status" ON "Feedbacks"("Status");
CREATE INDEX "IX_Feedbacks_CreatedAt" ON "Feedbacks"("CreatedAt");
```

---

## Security Model

```
┌─────────────────────────────────────────┐
│  Authentication & Authorization         │
└─────────────────────────────────────────┘

1. JWT Token Structure
   ┌──────────────────────┐
   │  Token Payload       │
   │                      │
   │  - User ID           │
   │  - Email             │
   │  - Name              │
   │  - Role Claim ★      │
   │  - Expiration        │
   └──────────────────────┘

2. Role-Based Authorization
   ┌─────────────────────────────────┐
   │  Endpoint Authorization         │
   │                                 │
   │  POST /feedback                 │
   │  ├─ [Authorize]                 │
   │  └─ Any authenticated user      │
   │                                 │
   │  GET /feedback                  │
   │  ├─ [Authorize(Roles="Admin")]  │
   │  └─ Admin only                  │
   │                                 │
   │  PATCH /feedback/{id}/status    │
   │  ├─ [Authorize(Roles="Admin")]  │
   │  └─ Admin only                  │
   └─────────────────────────────────┘

3. Data Access Control
   ┌────────────────────────────────┐
   │  Regular users can:            │
   │  ✓ Submit feedback             │
   │  ✗ View all feedbacks          │
   │  ✗ Update status               │
   └────────────────────────────────┘

   ┌────────────────────────────────┐
   │  Admin users can:              │
   │  ✓ Submit feedback             │
   │  ✓ View all feedbacks          │
   │  ✓ Update status               │
   │  ✓ Add admin response          │
   └────────────────────────────────┘
```

---

## Frontend Integration Points

### 1. Navigation/Menu Changes

```
Regular User Menu:
├── Dashboard
├── Transactions
├── Budgets
├── Categories
└── Settings
    └── Send Feedback ★ NEW

Admin User Menu:
├── Dashboard
├── Transactions
├── Budgets
├── Categories
├── Settings
│   └── Send Feedback
└── Admin Panel ★ NEW
    └── Feedback Management ★ NEW
```

### 2. Components to Create

```
src/
├── components/
│   ├── feedback/
│   │   ├── FeedbackForm.tsx          ★ NEW
│   │   ├── FeedbackTypeSelect.tsx    ★ NEW
│   │   └── FeedbackPrioritySelect.tsx ★ NEW
│   │
│   └── admin/
│       ├── FeedbackDashboard.tsx      ★ NEW
│       ├── FeedbackTable.tsx          ★ NEW
│       ├── FeedbackFilters.tsx        ★ NEW
│       ├── FeedbackDetailModal.tsx    ★ NEW
│       └── StatusBadge.tsx            ★ NEW
│
├── services/
│   └── feedbackService.ts             ★ NEW
│
├── types/
│   └── feedback.ts                    ★ NEW
│
└── hooks/
    └── useFeedback.ts                 ★ NEW
```

### 3. State Management

```typescript
// Redux/Context State Structure
interface FeedbackState {
  // User feedback submission
  submitting: boolean;
  submitError: string | null;

  // Admin feedback list
  feedbacks: FeedbackDto[];
  loading: boolean;
  error: string | null;

  // Pagination
  currentPage: number;
  pageSize: number;
  totalCount: number;

  // Filters
  filters: {
    searchTerm: string;
    type: FeedbackType | null;
    status: FeedbackStatus | null;
    priority: FeedbackPriority | null;
  };

  // Detail modal
  selectedFeedback: FeedbackDto | null;
  updating: boolean;
}
```

---

## Testing Checklist for Frontend Team

### User Features
- [ ] Can open feedback form
- [ ] Can select feedback type
- [ ] Can select priority
- [ ] Form validates subject (required, max 200 chars)
- [ ] Form validates description (required, max 2000 chars)
- [ ] Submit button shows loading state
- [ ] Success message appears after submission
- [ ] Form clears after successful submission
- [ ] Error messages display for validation errors
- [ ] Network errors handled gracefully

### Admin Features
- [ ] Admin menu item visible only for admin users
- [ ] Feedback table loads with data
- [ ] Pagination works correctly
- [ ] Search functionality works
- [ ] Type filter works
- [ ] Status filter works
- [ ] Priority filter works
- [ ] Date range filter works
- [ ] Sorting works (by date, status, priority)
- [ ] Can open feedback detail modal
- [ ] Can change status
- [ ] Can add admin response
- [ ] Update button shows loading state
- [ ] Success message after update
- [ ] Table refreshes after update
- [ ] Non-admin users get 403 error

### General
- [ ] JWT token includes role claim
- [ ] Role is extracted correctly
- [ ] Admin UI hidden from non-admin users
- [ ] Token expiration handled
- [ ] Responsive on mobile
- [ ] Accessible (keyboard nav, ARIA labels)

---

**Document Version:** 1.0
**Last Updated:** January 16, 2025
