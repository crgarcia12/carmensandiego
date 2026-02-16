@api
Feature: Session Management
  As a player, I want my game session to be managed automatically
  so that my progress is saved and I can resume where I left off.

  @smoke
  Scenario: Creating a new session
    Given fewer than 1000 active sessions exist
    When a new session is created via POST /api/sessions
    Then the response status is 201
    And the response contains an "id" matching the pattern "sess-" followed by a UUID
    And the response contains "createdAt" as a UTC timestamp
    And the response contains "status" equal to "active"
    And the response header "X-Session-Id" matches the response body "id"

  @smoke
  Scenario: Resuming an existing session
    Given a session "sess-resume-001" was created and is still active
    When the session is retrieved via GET /api/sessions/sess-resume-001
    Then the response status is 200
    And the response contains "id" equal to "sess-resume-001"
    And the response contains "status" equal to "active"
    And the response contains "lastAccessedAt" updated to the current time
    And the response contains "activeCaseId" if a case is in progress

  @api
  Scenario: Session expires after 24 hours of inactivity
    Given a session "sess-expire-001" has not been accessed for 25 hours
    When any API call is made with session "sess-expire-001"
    Then the response status is 410
    And the response body contains "error" with value "Session expired"
    And the response body contains "code" with value "SESSION_EXPIRED"

  @api
  Scenario: Expired session redirects client to create a new session
    Given the client has "sess-expire-002" stored in localStorage
    And the session "sess-expire-002" has expired
    When the client calls GET /api/sessions/sess-expire-002
    Then the response status is 410
    And the client clears "sess-expire-002" from localStorage
    And the client creates a new session via POST /api/sessions

  @api
  Scenario: Maximum concurrent sessions returns HTTP 503
    Given exactly 1000 active sessions exist
    When a new session is created via POST /api/sessions
    Then the response status is 503
    And the response body contains "error" with value "Server at capacity"
    And the response body contains "code" with value "MAX_SESSIONS_REACHED"
    And the response body contains "retryAfter" equal to 60
    And the response header "Retry-After" equals "60"

  @api
  Scenario: Deleting a session
    Given an active session "sess-delete-001" exists with an active case
    When the session is deleted via DELETE /api/sessions/sess-delete-001
    Then the response status is 204
    And the session is marked as "deleted"
    And the active case transitions to status "lost"

  @api
  Scenario: Deleting a session without an active case
    Given an active session "sess-delete-002" exists with no active case
    When the session is deleted via DELETE /api/sessions/sess-delete-002
    Then the response status is 204
    And the session is marked as "deleted"

  @api
  Scenario: Deleting a nonexistent session returns 404
    When a deletion is attempted via DELETE /api/sessions/sess-nonexistent
    Then the response status is 404

  @api
  Scenario: Deleting an already expired session returns 410
    Given session "sess-expired-del" has expired
    When a deletion is attempted via DELETE /api/sessions/sess-expired-del
    Then the response status is 410

  @api
  Scenario: Missing session ID header on protected endpoint returns 401
    When a request to GET /api/cases/case-001 is made without an X-Session-Id header
    Then the response status is 401
    And the response body contains "error" with value "Session ID required"
    And the response body contains "code" with value "MISSING_SESSION"

  @api
  Scenario: Invalid session ID format returns 400
    When a request is made with X-Session-Id header "not-a-valid-uuid"
    Then the response status is 400
    And the response body contains "error" with value "Invalid session ID format"
    And the response body contains "code" with value "INVALID_SESSION"

  @api
  Scenario: Last accessed timestamp updates on every API call
    Given an active session "sess-timestamp-001"
    When the player makes an API call at time T1
    And then makes another API call at time T2
    Then the session "lastAccessedAt" is updated to T2

  @api
  Scenario: Expired sessions are cleaned up after 48 hours
    Given a session "sess-cleanup-001" was marked expired 49 hours ago
    When the background cleanup process runs
    Then the session "sess-cleanup-001" is permanently deleted from storage

  @api
  Scenario: Resuming a deleted session returns 404
    Given session "sess-deleted-001" was previously deleted
    When the session is retrieved via GET /api/sessions/sess-deleted-001
    Then the response status is 404
