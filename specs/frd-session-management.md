# FRD: Session Management

| Field | Value |
|---|---|
| **Feature** | Session Management |
| **PRD Refs** | FR-6.1, FR-6.2, FR-6.3, FR-6.4 |
| **Dependencies** | None (foundation) |
| **Status** | Approved |

## 1. Overview

Session Management provides unique session identification, server-side game state persistence, automatic expiry after 24 hours of inactivity, session resume on reconnect, and a concurrency cap of 1000 simultaneous sessions to protect server resources.

## 2. User Stories

### US-SM-1: Create a Session
- **As a** player, **I want** a session to be created when I visit the game, **so that** my game progress is tracked across interactions.
- **Acceptance Criteria:**
  - A unique session ID is generated and returned.
  - The session is stored server-side.
  - The session ID is returned in the response body and as a header.

### US-SM-2: Resume a Session
- **As a** player, **I want** to resume my session if I close and reopen the browser, **so that** I do not lose my progress.
- **Acceptance Criteria:**
  - Given a valid session ID, the server returns the associated game state.
  - If the session is expired, the server returns 410 Gone.

### US-SM-3: Session Expiry
- **As a** system operator, **I want** sessions to expire after 24 hours of inactivity, **so that** server resources are reclaimed.
- **Acceptance Criteria:**
  - Sessions not accessed for 24 hours are marked expired.
  - Expired sessions return 410 Gone on any access attempt.
  - The `lastAccessedAt` timestamp updates on every API call within the session.

### US-SM-4: Concurrent Session Cap
- **As a** system operator, **I want** a maximum of 1000 concurrent sessions, **so that** the server does not become overloaded.
- **Acceptance Criteria:**
  - When 1000 active sessions exist and a new session is requested, HTTP 503 is returned.
  - The response includes a `Retry-After` header with value 60 (seconds).

## 3. Functional Requirements

### FRD-SM-1: Session Creation
- The system SHALL generate a unique session ID using UUID v4.
- The session SHALL be stored server-side with: `id`, `createdAt`, `lastAccessedAt`, `status`, and `gameState`.
- Initial `status` SHALL be `active`.
- Initial `gameState` SHALL be `null` (no case started).

### FRD-SM-2: Session ID Propagation
- The session ID SHALL be returned in the response body of POST /api/sessions.
- All subsequent API requests SHALL include the session ID via the `X-Session-Id` request header.
- If `X-Session-Id` is missing on a protected endpoint, the server SHALL return HTTP 401 Unauthorized.

### FRD-SM-3: Session Resume
- GET /api/sessions/{id} SHALL return the full session state including any active case reference.
- The `lastAccessedAt` timestamp SHALL be updated on every successful API call associated with the session.

### FRD-SM-4: Session Expiry
- A background cleanup process SHALL run every 5 minutes to mark sessions as `expired` if `lastAccessedAt` is older than 24 hours.
- Expired sessions SHALL NOT be deleted immediately — they are marked as `expired` and cleaned up after 48 hours.
- Any API call with an expired session ID SHALL return HTTP 410 Gone with body `{ "error": "Session expired", "code": "SESSION_EXPIRED" }`.

### FRD-SM-5: Concurrent Session Limit
- The system SHALL maintain a count of active (non-expired) sessions.
- When the count reaches 1000, POST /api/sessions SHALL return HTTP 503 Service Unavailable with a `Retry-After: 60` header.
- The response body SHALL be `{ "error": "Server at capacity", "code": "MAX_SESSIONS_REACHED", "retryAfter": 60 }`.

### FRD-SM-6: Session Deletion
- DELETE /api/sessions/{id} SHALL mark the session as `deleted` and free the concurrency slot.
- If the session has an active case, the case status SHALL transition to `lost` (abandoned).

### FRD-SM-7: Session Validation Middleware
- All API endpoints except POST /api/sessions SHALL validate the `X-Session-Id` header.
- Validation checks: header present, session exists, session not expired.
- Failed validation SHALL short-circuit the request with the appropriate error response.

## 4. API Specification

### POST /api/sessions
**Description:** Create a new game session.

**Request Body:** None

**Success Response (201 Created):**
```json
{
  "id": "sess-550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2025-01-15T10:30:00Z",
  "status": "active"
}
```

**Response Headers:**
- `X-Session-Id: sess-550e8400-e29b-41d4-a716-446655440000`

**Error Responses:**
- `503 Service Unavailable` — Max sessions reached. Includes `Retry-After: 60` header.

### GET /api/sessions/{id}
**Description:** Resume / retrieve an existing session.

**Success Response (200 OK):**
```json
{
  "id": "sess-550e8400-e29b-41d4-a716-446655440000",
  "createdAt": "2025-01-15T10:30:00Z",
  "lastAccessedAt": "2025-01-15T14:22:00Z",
  "status": "active",
  "activeCaseId": "case-abc123"
}
```

**Error Responses:**
- `404 Not Found` — Session ID does not exist.
- `410 Gone` — Session has expired.

### DELETE /api/sessions/{id}
**Description:** End a session and abandon any active case.

**Success Response (204 No Content)**

**Error Responses:**
- `404 Not Found` — Session ID does not exist.
- `410 Gone` — Session already expired.

## 5. Data Models

### GameSession
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique, format: `sess-{uuid}` |
| `createdAt` | datetime | UTC, set on creation |
| `lastAccessedAt` | datetime | UTC, updated on every API call |
| `status` | enum | `active`, `expired`, `deleted` |
| `activeCaseId` | string? | Null until a case is started |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-SM-1 | Session ID not provided on protected endpoint | HTTP 401 with `{ "error": "Session ID required", "code": "MISSING_SESSION" }` |
| EC-SM-2 | Session ID format is invalid | HTTP 400 with `{ "error": "Invalid session ID format", "code": "INVALID_SESSION" }` |
| EC-SM-3 | Session expired (>24h inactive) | HTTP 410 with `{ "error": "Session expired", "code": "SESSION_EXPIRED" }` |
| EC-SM-4 | Max sessions reached (1000) | HTTP 503 with `Retry-After: 60` header |
| EC-SM-5 | Corrupted session data on server | Server logs error; returns HTTP 500 with `{ "error": "Session data corrupted", "code": "SESSION_CORRUPT" }`. Session is marked `expired`. |
| EC-SM-6 | Delete a session with active case | Case status transitions to `lost`; session marked `deleted` |
| EC-SM-7 | Resume a deleted session | HTTP 404 (treated as not found) |
| EC-SM-8 | Concurrent requests on same session | Last-write-wins with optimistic concurrency; no data corruption |

## 7. Acceptance Criteria

1. **Given** fewer than 1000 active sessions, **when** POST /api/sessions is called, **then** a new session with a unique ID and status `active` is returned with HTTP 201.
2. **Given** 1000 active sessions exist, **when** POST /api/sessions is called, **then** HTTP 503 is returned with `Retry-After: 60`.
3. **Given** a valid session ID, **when** GET /api/sessions/{id} is called, **then** the session state is returned with HTTP 200 and `lastAccessedAt` is updated.
4. **Given** a session inactive for more than 24 hours, **when** any API call is made with that session ID, **then** HTTP 410 is returned with code `SESSION_EXPIRED`.
5. **Given** a session with an active case, **when** DELETE /api/sessions/{id} is called, **then** the case status becomes `lost` and the session is marked `deleted`.
6. **Given** no `X-Session-Id` header on a protected endpoint, **when** the request is processed, **then** HTTP 401 is returned.
7. **Given** an expired session, **when** the cleanup process runs, **then** sessions expired for more than 48 hours are permanently deleted.
8. **Given** a valid session, **when** any API call is made, **then** `lastAccessedAt` is updated to the current UTC timestamp.
