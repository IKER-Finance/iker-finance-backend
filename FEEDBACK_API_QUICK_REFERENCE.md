# Feedback API - Quick Reference

## Endpoints

### 1. Submit Feedback
```http
POST /api/feedback
Authorization: Bearer {token}
```
**Body:**
```json
{
  "type": 1,           // 1=Bug, 2=Feature, 3=Improvement, 4=Question, 5=Complaint
  "subject": "string", // Max 200 chars
  "description": "string", // Max 2000 chars
  "priority": 2        // 1=Low, 2=Medium, 3=High, 4=Critical
}
```

---

### 2. Get All Feedbacks (Admin Only)
```http
GET /api/feedback?pageNumber=1&pageSize=10&status=1&sortBy=CreatedAt&sortOrder=desc
Authorization: Bearer {admin_token}
```

**Query Parameters:**
- `pageNumber` (default: 1)
- `pageSize` (default: 10, max: 100)
- `searchTerm` (optional)
- `type` (optional: 1-5)
- `status` (optional: 1-5)
- `priority` (optional: 1-4)
- `startDate` (optional: ISO 8601)
- `endDate` (optional: ISO 8601)
- `sortBy` (default: "CreatedAt")
- `sortOrder` (default: "desc")

---

### 3. Update Feedback Status (Admin Only)
```http
PATCH /api/feedback/{id}/status
Authorization: Bearer {admin_token}
```
**Body:**
```json
{
  "status": 2,                    // 1=Open, 2=InProgress, 3=Responded, 4=Resolved, 5=Closed
  "adminResponse": "string"       // Optional, max 2000 chars
}
```

---

## Enums Reference

| FeedbackType | Value | FeedbackStatus | Value | FeedbackPriority | Value |
|--------------|-------|----------------|-------|------------------|-------|
| Bug          | 1     | Open           | 1     | Low              | 1     |
| Feature      | 2     | InProgress     | 2     | Medium           | 2     |
| Improvement  | 3     | Responded      | 3     | High             | 3     |
| Question     | 4     | Resolved       | 4     | Critical         | 4     |
| Complaint    | 5     | Closed         | 5     |                  |       |

---

## Admin Credentials (Testing)

```
Email: admin@ikerfinance.com
Password: Admin@123456
```

---

## Role Detection

```typescript
// Decode JWT and check role claim
const role = decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
const isAdmin = role === 'Admin';
```

---

## Response Example

```json
{
  "id": 1,
  "userId": "guid",
  "userName": "John Doe",
  "userEmail": "john@example.com",
  "type": 1,
  "subject": "Bug report",
  "description": "Description here",
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

---

## Paginated Response

```json
{
  "data": [...],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## HTTP Status Codes

- `200` - Success (GET/PATCH)
- `201` - Created (POST)
- `400` - Validation Error
- `401` - Unauthorized
- `403` - Forbidden (Admin only)
- `404` - Not Found
- `500` - Server Error
