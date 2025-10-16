# Feedback Feature - Frontend Integration Guide

## Overview

The backend now supports a complete feedback system where users can submit feedback and admins can manage it. This document provides all the information your frontend team needs to integrate with the new API endpoints.

---

## Table of Contents

1. [Authentication Changes](#authentication-changes)
2. [Feedback API Endpoints](#feedback-api-endpoints)
3. [Data Models](#data-models)
4. [User Interface Requirements](#user-interface-requirements)
5. [Example API Calls](#example-api-calls)
6. [Error Handling](#error-handling)

---

## Authentication Changes

### Role-Based Access Control

The authentication system now includes **role claims** in JWT tokens. After login/registration, the JWT token will contain role information.

#### Updated Auth Response

When users login or register, the JWT token now includes roles:

**Decoded JWT Token Structure:**
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "user-id-here",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@example.com",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "John Doe",
  "FirstName": "John",
  "LastName": "Doe",
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "User",
  "exp": 1234567890,
  "iss": "IkerFinance",
  "aud": "IkerFinance"
}
```

**For Admin Users:**
```json
{
  ...
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Admin",
  ...
}
```

#### Frontend Changes Needed

1. **Parse Role from JWT Token**
   - Extract the role claim from the decoded JWT token
   - Store the user's role in your application state (Redux, Context, etc.)
   - Use the role to conditionally show/hide admin features

2. **Admin User Credentials** (for testing)
   - Email: `admin@ikerfinance.com`
   - Password: `Admin@123456`

3. **Role Check Example** (JavaScript/TypeScript)
```typescript
import jwt_decode from 'jwt-decode';

interface DecodedToken {
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string;
  // ... other claims
}

function getUserRole(token: string): string {
  const decoded = jwt_decode<DecodedToken>(token);
  return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
}

function isAdmin(token: string): boolean {
  return getUserRole(token) === 'Admin';
}
```

---

## Feedback API Endpoints

### Base URL
All endpoints are prefixed with: `/api/feedback`

### 1. Submit Feedback (User Endpoint)

**Endpoint:** `POST /api/feedback`

**Authentication:** Required (any authenticated user)

**Request Headers:**
```http
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
```

**Request Body:**
```json
{
  "type": 1,
  "subject": "Bug in transaction page",
  "description": "When I try to add a transaction, the page freezes.",
  "priority": 2
}
```

**Field Descriptions:**

| Field | Type | Required | Description | Valid Values |
|-------|------|----------|-------------|--------------|
| `type` | integer | Yes | Type of feedback | 1=Bug, 2=Feature, 3=Improvement, 4=Question, 5=Complaint |
| `subject` | string | Yes | Brief summary | Max 200 characters |
| `description` | string | Yes | Detailed description | Max 2000 characters |
| `priority` | integer | Yes | Priority level | 1=Low, 2=Medium, 3=High, 4=Critical |

**Success Response (201 Created):**
```json
{
  "id": 1,
  "userId": "user-guid-here",
  "userName": "John Doe",
  "userEmail": "john@example.com",
  "type": 1,
  "subject": "Bug in transaction page",
  "description": "When I try to add a transaction, the page freezes.",
  "priority": 2,
  "status": 1,
  "adminResponse": null,
  "respondedByUserId": null,
  "respondedByUserName": null,
  "responseDate": null,
  "createdAt": "2025-01-16T10:30:00Z",
  "updatedAt": "2025-01-16T10:30:00Z"
}
```

**Error Response (400 Bad Request):**
```json
{
  "error": "Validation failed",
  "errors": {
    "Subject": ["Subject is required"],
    "Description": ["Description cannot exceed 2000 characters"]
  },
  "statusCode": 400
}
```

---

### 2. Get All Feedbacks (Admin Only)

**Endpoint:** `GET /api/feedback`

**Authentication:** Required (Admin role only)

**Request Headers:**
```http
Authorization: Bearer {ADMIN_JWT_TOKEN}
```

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageNumber` | integer | No | 1 | Page number |
| `pageSize` | integer | No | 10 | Items per page (max 100) |
| `searchTerm` | string | No | - | Search in subject and description |
| `type` | integer | No | - | Filter by feedback type (1-5) |
| `status` | integer | No | - | Filter by status (1-5) |
| `priority` | integer | No | - | Filter by priority (1-4) |
| `startDate` | string | No | - | Filter by start date (ISO 8601) |
| `endDate` | string | No | - | Filter by end date (ISO 8601) |
| `sortBy` | string | No | "CreatedAt" | Sort field (CreatedAt, Status, Priority) |
| `sortOrder` | string | No | "desc" | Sort order (asc, desc) |

**Example Request:**
```http
GET /api/feedback?pageNumber=1&pageSize=20&status=1&sortBy=CreatedAt&sortOrder=desc
```

**Success Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "userId": "user-guid-here",
      "userName": "John Doe",
      "userEmail": "john@example.com",
      "type": 1,
      "subject": "Bug in transaction page",
      "description": "When I try to add a transaction, the page freezes.",
      "priority": 2,
      "status": 1,
      "adminResponse": null,
      "respondedByUserId": null,
      "respondedByUserName": null,
      "responseDate": null,
      "createdAt": "2025-01-16T10:30:00Z",
      "updatedAt": "2025-01-16T10:30:00Z"
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

**Error Response (403 Forbidden - Non-Admin User):**
```json
{
  "error": "Forbidden",
  "statusCode": 403
}
```

---

### 3. Update Feedback Status (Admin Only)

**Endpoint:** `PATCH /api/feedback/{id}/status`

**Authentication:** Required (Admin role only)

**Request Headers:**
```http
Authorization: Bearer {ADMIN_JWT_TOKEN}
Content-Type: application/json
```

**URL Parameters:**
- `{id}`: Feedback ID (integer)

**Request Body:**
```json
{
  "status": 2,
  "adminResponse": "Thank you for reporting this issue. We're working on a fix and it will be released in the next update."
}
```

**Field Descriptions:**

| Field | Type | Required | Description | Valid Values |
|-------|------|----------|-------------|--------------|
| `status` | integer | Yes | New status | 1=Open, 2=InProgress, 3=Responded, 4=Resolved, 5=Closed |
| `adminResponse` | string | No | Admin's response message | Max 2000 characters |

**Success Response (200 OK):**
```json
{
  "id": 1,
  "userId": "user-guid-here",
  "userName": "John Doe",
  "userEmail": "john@example.com",
  "type": 1,
  "subject": "Bug in transaction page",
  "description": "When I try to add a transaction, the page freezes.",
  "priority": 2,
  "status": 2,
  "adminResponse": "Thank you for reporting this issue. We're working on a fix and it will be released in the next update.",
  "respondedByUserId": "admin-guid-here",
  "respondedByUserName": "System Admin",
  "responseDate": "2025-01-16T11:45:00Z",
  "createdAt": "2025-01-16T10:30:00Z",
  "updatedAt": "2025-01-16T11:45:00Z"
}
```

**Error Responses:**

**404 Not Found:**
```json
{
  "error": "Feedback not found",
  "statusCode": 404
}
```

**403 Forbidden:**
```json
{
  "error": "Forbidden",
  "statusCode": 403
}
```

---

## Data Models

### Enums

#### FeedbackType
```typescript
enum FeedbackType {
  Bug = 1,
  Feature = 2,
  Improvement = 3,
  Question = 4,
  Complaint = 5
}
```

#### FeedbackStatus
```typescript
enum FeedbackStatus {
  Open = 1,
  InProgress = 2,
  Responded = 3,
  Resolved = 4,
  Closed = 5
}
```

#### FeedbackPriority
```typescript
enum FeedbackPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}
```

### TypeScript Interfaces

```typescript
interface FeedbackDto {
  id: number;
  userId: string;
  userName: string;
  userEmail: string;
  type: FeedbackType;
  subject: string;
  description: string;
  priority: FeedbackPriority;
  status: FeedbackStatus;
  adminResponse: string | null;
  respondedByUserId: string | null;
  respondedByUserName: string | null;
  responseDate: string | null; // ISO 8601 datetime
  createdAt: string; // ISO 8601 datetime
  updatedAt: string; // ISO 8601 datetime
}

interface PaginatedFeedbackResponse {
  data: FeedbackDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface CreateFeedbackRequest {
  type: FeedbackType;
  subject: string;
  description: string;
  priority: FeedbackPriority;
}

interface UpdateFeedbackStatusRequest {
  status: FeedbackStatus;
  adminResponse?: string;
}
```

---

## User Interface Requirements

### For Regular Users

#### 1. Feedback Submission Form

**Location:** Settings page or Help section

**Required Fields:**
- **Type** (Dropdown/Select):
  - Bug
  - Feature Request
  - Improvement
  - Question
  - Complaint

- **Subject** (Text input):
  - Required
  - Max 200 characters
  - Placeholder: "Brief summary of your feedback"

- **Description** (Textarea):
  - Required
  - Max 2000 characters
  - Placeholder: "Please provide detailed information..."

- **Priority** (Dropdown/Select):
  - Low
  - Medium
  - High
  - Critical

- **Submit Button**

**UI Mockup:**
```
┌─────────────────────────────────────────┐
│  Submit Feedback                         │
├─────────────────────────────────────────┤
│                                          │
│  Type: [Bug ▼]                          │
│                                          │
│  Priority: [Medium ▼]                   │
│                                          │
│  Subject:                                │
│  [________________________]              │
│                                          │
│  Description:                            │
│  ┌────────────────────────────────────┐ │
│  │                                    │ │
│  │                                    │ │
│  │                                    │ │
│  └────────────────────────────────────┘ │
│                                          │
│  [Cancel]  [Submit Feedback]             │
└─────────────────────────────────────────┘
```

**Success Message:**
"Thank you for your feedback! We've received your submission and will review it shortly."

**Error Handling:**
- Show validation errors inline
- Display network errors with retry option

---

### For Admin Users

#### 2. Feedback Management Dashboard

**Location:** Admin Panel

**Features Needed:**

**A. Feedback List Table**

Columns:
- ID
- User (name + email)
- Type (with badge/color)
- Subject
- Priority (with badge/color)
- Status (with badge/color)
- Created Date
- Actions (View/Update)

**B. Filters & Search**

- Search box (searches subject and description)
- Type filter dropdown
- Status filter dropdown
- Priority filter dropdown
- Date range picker
- Sort by: Created Date, Status, Priority
- Sort order: Ascending/Descending

**C. Pagination**

- Items per page selector (10, 20, 50, 100)
- Page navigation
- Total count display

**D. Feedback Detail Modal**

When admin clicks on a feedback item:

```
┌─────────────────────────────────────────────────┐
│  Feedback Details                          [X]   │
├─────────────────────────────────────────────────┤
│                                                  │
│  Submitted by: John Doe (john@example.com)      │
│  Date: Jan 16, 2025 10:30 AM                    │
│                                                  │
│  Type: [Bug]  Priority: [High]                  │
│                                                  │
│  Subject:                                        │
│  Bug in transaction page                        │
│                                                  │
│  Description:                                    │
│  When I try to add a transaction, the page      │
│  freezes and I have to refresh...               │
│                                                  │
│  ─────────────────────────────────────────      │
│                                                  │
│  Status: [InProgress ▼]                         │
│                                                  │
│  Admin Response (optional):                     │
│  ┌────────────────────────────────────────────┐│
│  │ Thank you for reporting...                 ││
│  │                                            ││
│  └────────────────────────────────────────────┘│
│                                                  │
│  Responded by: System Admin                     │
│  Response Date: Jan 16, 2025 11:45 AM           │
│                                                  │
│  [Cancel]  [Update Status]                      │
└─────────────────────────────────────────────────┘
```

**Status Badge Colors (Recommended):**
- Open: Blue (#3B82F6)
- InProgress: Yellow (#EAB308)
- Responded: Purple (#A855F7)
- Resolved: Green (#22C55E)
- Closed: Gray (#6B7280)

**Priority Badge Colors (Recommended):**
- Low: Gray (#6B7280)
- Medium: Blue (#3B82F6)
- High: Orange (#F97316)
- Critical: Red (#EF4444)

**Type Badge Colors (Recommended):**
- Bug: Red (#EF4444)
- Feature: Green (#22C55E)
- Improvement: Blue (#3B82F6)
- Question: Purple (#A855F7)
- Complaint: Orange (#F97316)

---

## Example API Calls

### Using Fetch API

#### Submit Feedback (User)
```javascript
async function submitFeedback(token, feedbackData) {
  const response = await fetch('https://api.ikerfinance.com/api/feedback', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      type: feedbackData.type,
      subject: feedbackData.subject,
      description: feedbackData.description,
      priority: feedbackData.priority
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to submit feedback');
  }

  return await response.json();
}

// Usage
try {
  const result = await submitFeedback(userToken, {
    type: 1, // Bug
    subject: "Login button not working",
    description: "When I click the login button, nothing happens.",
    priority: 3 // High
  });
  console.log('Feedback submitted:', result);
} catch (error) {
  console.error('Error:', error.message);
}
```

#### Get Feedbacks (Admin)
```javascript
async function getFeedbacks(adminToken, params = {}) {
  const queryParams = new URLSearchParams({
    pageNumber: params.pageNumber || 1,
    pageSize: params.pageSize || 10,
    ...(params.searchTerm && { searchTerm: params.searchTerm }),
    ...(params.status && { status: params.status }),
    ...(params.type && { type: params.type }),
    ...(params.priority && { priority: params.priority }),
    sortBy: params.sortBy || 'CreatedAt',
    sortOrder: params.sortOrder || 'desc'
  });

  const response = await fetch(
    `https://api.ikerfinance.com/api/feedback?${queryParams}`,
    {
      headers: {
        'Authorization': `Bearer ${adminToken}`
      }
    }
  );

  if (!response.ok) {
    throw new Error('Failed to fetch feedbacks');
  }

  return await response.json();
}

// Usage
const feedbacks = await getFeedbacks(adminToken, {
  pageNumber: 1,
  pageSize: 20,
  status: 1, // Open
  sortBy: 'CreatedAt',
  sortOrder: 'desc'
});
```

#### Update Feedback Status (Admin)
```javascript
async function updateFeedbackStatus(adminToken, feedbackId, statusData) {
  const response = await fetch(
    `https://api.ikerfinance.com/api/feedback/${feedbackId}/status`,
    {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        status: statusData.status,
        adminResponse: statusData.adminResponse
      })
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Failed to update feedback');
  }

  return await response.json();
}

// Usage
const updated = await updateFeedbackStatus(adminToken, 1, {
  status: 4, // Resolved
  adminResponse: "This issue has been fixed in version 2.1.0"
});
```

### Using Axios

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.ikerfinance.com/api'
});

// Submit Feedback
const submitFeedback = async (token, data) => {
  return await api.post('/feedback', data, {
    headers: { Authorization: `Bearer ${token}` }
  });
};

// Get Feedbacks (Admin)
const getFeedbacks = async (adminToken, params) => {
  return await api.get('/feedback', {
    headers: { Authorization: `Bearer ${adminToken}` },
    params
  });
};

// Update Status (Admin)
const updateStatus = async (adminToken, id, data) => {
  return await api.patch(`/feedback/${id}/status`, data, {
    headers: { Authorization: `Bearer ${adminToken}` }
  });
};
```

---

## Error Handling

### HTTP Status Codes

| Code | Meaning | When It Happens |
|------|---------|-----------------|
| 200 | OK | Successful GET/PATCH request |
| 201 | Created | Feedback successfully submitted |
| 400 | Bad Request | Validation errors in request body |
| 401 | Unauthorized | Missing or invalid JWT token |
| 403 | Forbidden | Non-admin trying to access admin endpoint |
| 404 | Not Found | Feedback ID doesn't exist |
| 500 | Server Error | Internal server error |

### Error Response Format

```json
{
  "error": "Error message",
  "errors": {
    "FieldName": ["Validation error message"]
  },
  "statusCode": 400
}
```

### Recommended Error Handling Strategy

```javascript
async function handleApiCall(apiFunction) {
  try {
    const response = await apiFunction();
    return { success: true, data: response.data };
  } catch (error) {
    if (error.response) {
      // Server responded with error
      const { status, data } = error.response;

      switch (status) {
        case 400:
          return {
            success: false,
            message: 'Please check your input',
            errors: data.errors
          };
        case 401:
          return {
            success: false,
            message: 'Please login again',
            shouldLogout: true
          };
        case 403:
          return {
            success: false,
            message: 'You do not have permission to perform this action'
          };
        case 404:
          return {
            success: false,
            message: 'Feedback not found'
          };
        default:
          return {
            success: false,
            message: 'An error occurred. Please try again.'
          };
      }
    } else if (error.request) {
      // Network error
      return {
        success: false,
        message: 'Network error. Please check your connection.'
      };
    } else {
      return {
        success: false,
        message: 'An unexpected error occurred'
      };
    }
  }
}
```

---

## Testing Credentials

**Admin User:**
- Email: `admin@ikerfinance.com`
- Password: `Admin@123456`
- Role: Admin

**Regular User:**
- Register a new user through `/api/auth/register`
- Will automatically receive "User" role

---

## Notes & Best Practices

1. **Token Storage:** Store JWT tokens securely (httpOnly cookies or secure localStorage)

2. **Token Expiration:** JWT tokens expire after 24 hours. Implement token refresh or re-login flow

3. **Role Check:** Always check user role before showing admin UI elements

4. **Validation:** Validate form inputs on frontend before submission to improve UX

5. **Loading States:** Show loading indicators during API calls

6. **Pagination:** Implement proper pagination UI with page size selector

7. **Real-time Updates:** Consider implementing WebSocket or polling for real-time feedback status updates (future enhancement)

8. **Accessibility:** Ensure feedback forms and admin dashboard are accessible (ARIA labels, keyboard navigation)

9. **Mobile Responsive:** Both user and admin interfaces should be mobile-friendly

10. **Notifications:** Consider showing toast notifications for successful submissions/updates

---

## Support

For questions or issues during integration, please contact:
- Backend Team Lead: [Your Contact Info]
- API Documentation: [Swagger/OpenAPI URL if available]

---

**Document Version:** 1.0
**Last Updated:** January 16, 2025
**API Version:** v1
