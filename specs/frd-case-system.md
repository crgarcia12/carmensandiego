# FRD: Case System

| Field | Value |
|---|---|
| **Feature** | Case System |
| **PRD Refs** | FR-1.1, FR-1.2, FR-1.3, FR-1.4, FR-1.5 |
| **Dependencies** | None (foundation) |
| **Status** | Approved |

## 1. Overview

The Case System manages the lifecycle of a detective case — from generation through investigation to resolution. It provides the foundational data model for cases, loads demo case configurations from JSON, enforces step/time limits, evaluates win/lose conditions, and renders a summary screen upon case completion.

## 2. User Stories

### US-CS-1: Start a New Case
- **As a** player, **I want** to start a new detective case, **so that** I receive a briefing with a stolen treasure, a villain trail, and a set of cities to investigate.
- **Acceptance Criteria:**
  - A new case is created with a unique ID.
  - The case includes a stolen treasure name and description.
  - The case includes a trail of 4–6 cities.
  - The case starts with 10 remaining steps.
  - The player receives a briefing narrative.

### US-CS-2: View Case State
- **As a** player, **I want** to view my current case state at any time, **so that** I know how many steps remain and what cities I have visited.
- **Acceptance Criteria:**
  - The response includes case ID, current city, visited cities, remaining steps, and case status.
  - Status is one of: `active`, `won`, `lost`.

### US-CS-3: View Case Summary
- **As a** player, **I want** to see a summary when my case ends, **so that** I know whether I won or lost and can review my investigation path.
- **Acceptance Criteria:**
  - Summary includes outcome (`won` or `lost`), cities visited, steps used, the correct suspect, and the player's warrant (if issued).
  - Summary is only available when case status is `won` or `lost`.

## 3. Functional Requirements

### FRD-CS-1: Case Generation
- The system SHALL load case configurations from a static JSON file (`data/cases.json`).
- Each case config SHALL include: `id`, `title`, `briefing`, `stolenTreasure`, `trail` (array of 4–6 city IDs), `correctSuspectId`, and `decoyTravel` mappings.
- When a new case is requested, the system SHALL select a case config (round-robin or random from available configs).

### FRD-CS-2: Case Initialization
- A new case SHALL be initialized with:
  - `status`: `active`
  - `remainingSteps`: 10
  - `currentCityIndex`: 0 (first city in the trail)
  - `visitedCities`: [first city]
  - `warrantIssued`: false
- The case SHALL be associated with the requesting session.

### FRD-CS-3: Step Limit Enforcement
- Each travel action SHALL decrement `remainingSteps` by 1.
- When `remainingSteps` reaches 0 and no correct warrant has been issued, the case status SHALL transition to `lost`.

### FRD-CS-4: Win Condition
- The case status SHALL transition to `won` when the player issues a warrant for the correct suspect while in the final city of the trail.

### FRD-CS-5: Lose Conditions
- The case status SHALL transition to `lost` when:
  - `remainingSteps` reaches 0 without a correct warrant, OR
  - The player issues a warrant for the wrong suspect, OR
  - The player issues a warrant in the wrong city (not the final trail city).

### FRD-CS-6: Case Immutability After Completion
- Once a case status is `won` or `lost`, no further travel, chat, or warrant actions SHALL be accepted. The API SHALL return HTTP 409 Conflict.

### FRD-CS-7: Case Summary Generation
- The summary SHALL include: `outcome`, `citiesVisited`, `stepsUsed`, `totalSteps` (10), `correctSuspect`, `playerWarrant` (suspect ID and city, or null), `stolenTreasure`.

## 4. API Specification

### POST /api/cases
**Description:** Start a new case for the current session.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Request Body:** None

**Success Response (201 Created):**
```json
{
  "id": "case-abc123",
  "title": "The Stolen Sapphire of Siam",
  "briefing": "A priceless sapphire has vanished from the Bangkok National Museum...",
  "stolenTreasure": {
    "name": "Sapphire of Siam",
    "description": "A 200-carat blue sapphire dating to the 15th century"
  },
  "currentCity": "bangkok",
  "remainingSteps": 10,
  "status": "active"
}
```

**Error Responses:**
- `400 Bad Request` — Missing session ID header.
- `404 Not Found` — No case configurations available.
- `409 Conflict` — Session already has an active case.

### GET /api/cases/{id}
**Description:** Get the current state of a case.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Success Response (200 OK):**
```json
{
  "id": "case-abc123",
  "title": "The Stolen Sapphire of Siam",
  "currentCity": "bangkok",
  "visitedCities": ["bangkok"],
  "remainingSteps": 10,
  "status": "active",
  "warrantIssued": false
}
```

**Error Responses:**
- `403 Forbidden` — Case does not belong to this session.
- `404 Not Found` — Case ID not found.

### GET /api/cases/{id}/summary
**Description:** Get the summary of a completed case.

**Success Response (200 OK):**
```json
{
  "outcome": "won",
  "citiesVisited": ["bangkok", "tokyo", "paris", "cairo"],
  "stepsUsed": 7,
  "totalSteps": 10,
  "correctSuspect": {
    "id": "suspect-carmen",
    "name": "Carmen Sandiego"
  },
  "playerWarrant": {
    "suspectId": "suspect-carmen",
    "city": "cairo"
  },
  "stolenTreasure": {
    "name": "Sapphire of Siam",
    "description": "A 200-carat blue sapphire dating to the 15th century"
  }
}
```

**Error Responses:**
- `400 Bad Request` — Case is still active (not yet completed).
- `404 Not Found` — Case ID not found.

## 5. Data Models

### Case
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique, generated (e.g., `case-{uuid}`) |
| `configId` | string | References CaseConfig.id |
| `sessionId` | string | References GameSession.id |
| `title` | string | From CaseConfig |
| `briefing` | string | From CaseConfig |
| `stolenTreasure` | StolenTreasure | Required |
| `trail` | string[] | 4–6 city IDs, from CaseConfig |
| `currentCityIndex` | int | 0-based index into trail; 0 ≤ index < trail.length |
| `visitedCities` | string[] | Ordered list of visited city IDs |
| `remainingSteps` | int | 0–10, starts at 10 |
| `status` | enum | `active`, `won`, `lost` |
| `warrantIssued` | bool | Default false |
| `correctSuspectId` | string | From CaseConfig |
| `playerWarrant` | Warrant? | Null until warrant issued |

### CaseConfig
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique within config file |
| `title` | string | Required, non-empty |
| `briefing` | string | Required, non-empty |
| `stolenTreasure` | StolenTreasure | Required |
| `trail` | string[] | 4–6 city IDs |
| `correctSuspectId` | string | Must match a suspect ID |
| `decoyTravel` | Map<string, string[]> | City ID → array of 2 decoy city IDs |

### StolenTreasure
| Field | Type | Constraints |
|---|---|---|
| `name` | string | Required, non-empty |
| `description` | string | Required, non-empty |

### CaseOutcome
| Field | Type | Constraints |
|---|---|---|
| `outcome` | enum | `won`, `lost` |
| `citiesVisited` | string[] | Non-empty |
| `stepsUsed` | int | 1–10 |
| `totalSteps` | int | Always 10 |
| `correctSuspect` | Suspect | Required |
| `playerWarrant` | Warrant? | Null if no warrant issued |
| `stolenTreasure` | StolenTreasure | Required |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-CS-1 | No case configs in `data/cases.json` | POST /api/cases returns 404 with message "No case configurations available" |
| EC-CS-2 | Case config JSON is malformed | Server logs error; POST /api/cases returns 500 with message "Failed to load case data" |
| EC-CS-3 | Session already has an active case | POST /api/cases returns 409 with message "Active case already exists" |
| EC-CS-4 | Case already completed (won/lost) | Any action endpoint returns 409 with message "Case is already completed" |
| EC-CS-5 | Case ID does not exist | GET returns 404 with message "Case not found" |
| EC-CS-6 | Summary requested for active case | GET /api/cases/{id}/summary returns 400 with message "Case is still active" |
| EC-CS-7 | Case config references nonexistent city | Server logs warning; city is skipped from trail during initialization |

## 7. Acceptance Criteria

1. **Given** no active case exists for the session, **when** POST /api/cases is called, **then** a new case is created with status `active`, 10 remaining steps, and a valid briefing.
2. **Given** an active case exists, **when** POST /api/cases is called again, **then** HTTP 409 is returned.
3. **Given** a case with 1 remaining step, **when** the player travels and does not issue a correct warrant, **then** the case status becomes `lost`.
4. **Given** a case where the player is in the final trail city, **when** a warrant is issued for the correct suspect, **then** the case status becomes `won`.
5. **Given** a case where the player issues a warrant for the wrong suspect, **then** the case status becomes `lost`.
6. **Given** a completed case, **when** GET /api/cases/{id}/summary is called, **then** the full summary is returned with outcome, cities, steps, suspect, and treasure.
7. **Given** an active case, **when** GET /api/cases/{id}/summary is called, **then** HTTP 400 is returned.
8. **Given** no case configurations are available, **when** POST /api/cases is called, **then** HTTP 404 is returned.
