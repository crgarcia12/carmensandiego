# FRD: Suspect Dossier & Warrants

| Field | Value |
|---|---|
| **Feature** | Suspect Dossier & Warrants |
| **PRD Refs** | FR-4.1, FR-4.2, FR-4.3, FR-4.4 |
| **Dependencies** | frd-case-system |
| **Status** | Approved |

## 1. Overview

The Suspect Dossier provides the player with a list of 10+ suspects, each with distinguishing traits. The player can view the dossier at any time during an active case, cross-reference NPC clues with suspect traits, and issue a one-time, irreversible arrest warrant. The warrant determines the case outcome: correct suspect in the correct city = win; anything else = lose.

## 2. User Stories

### US-SD-1: View Suspect Dossier
- **As a** player, **I want** to browse the list of suspects and their traits, **so that** I can narrow down who committed the crime.
- **Acceptance Criteria:**
  - The dossier shows all 10+ suspects with their name and photo key.
  - Each suspect's traits (hair color, eye color, hobby, food, vehicle, feature) are visible.
  - The dossier is accessible at any time during an active case.

### US-SD-2: Issue Arrest Warrant
- **As a** player, **I want** to issue an arrest warrant for a suspect, **so that** I can attempt to solve the case.
- **Acceptance Criteria:**
  - The player selects exactly one suspect from the dossier.
  - A confirmation dialog is shown: "Are you sure you want to issue a warrant for [Suspect Name]? This action is irreversible."
  - Once confirmed, the warrant is submitted and cannot be changed.
  - If the suspect is correct AND the player is in the final trail city, the case is won.
  - Otherwise, the case is lost.

### US-SD-3: Review Warrant Outcome
- **As a** player, **I want** to see the result of my warrant, **so that** I know whether I caught the right suspect.
- **Acceptance Criteria:**
  - Correct warrant: "Congratulations! You've apprehended [Suspect Name]!" with case status `won`.
  - Wrong suspect: "That's not the right suspect. [Correct Suspect Name] got away!" with case status `lost`.
  - Wrong city: "You issued the warrant in the wrong city. The suspect escaped!" with case status `lost`.

## 3. Functional Requirements

### FRD-SD-1: Suspect Data
- The system SHALL define 10+ suspects in `data/suspects.json`.
- Each suspect SHALL have: `id`, `name`, `photoKey`, and `traits` object.
- The `traits` object SHALL include: `hairColor`, `eyeColor`, `hobby`, `favoriteFood`, `vehicle`, `distinguishingFeature`.
- All trait values SHALL be strings.

### FRD-SD-2: Dossier Retrieval
- GET /api/cases/{id}/suspects SHALL return the complete list of suspects with all traits.
- The suspect list SHALL be the same for all cases (shared suspect pool).
- The response SHALL NOT indicate which suspect is correct.

### FRD-SD-3: Warrant Issuance
- POST /api/cases/{id}/warrant SHALL:
  1. Validate the case is `active`.
  2. Validate no warrant has been previously issued for this case.
  3. Validate the `suspectId` exists in the suspect list.
  4. Record the warrant: `suspectId`, `cityId` (current city), `issuedAt` timestamp.
  5. Set `warrantIssued` to true on the case.
  6. Evaluate the warrant:
     - If `suspectId` matches `correctSuspectId` AND current city is the final trail city → set case status to `won`.
     - Otherwise → set case status to `lost`.
  7. Return the warrant result.

### FRD-SD-4: Warrant Irreversibility
- Once a warrant is issued, it SHALL NOT be modifiable or retractable.
- Subsequent POST /api/cases/{id}/warrant calls SHALL return HTTP 409.

### FRD-SD-5: Warrant Timing
- A warrant can be issued at any point during an active case (not just at the final city).
- Issuing a warrant in a non-final city SHALL result in a loss (wrong city).
- This allows the player to make strategic decisions about when to commit.

### FRD-SD-6: Suspect Trait Categories
- The following trait categories SHALL be used consistently across all suspects and NPC clues:
  - `hairColor`: e.g., "red", "black", "blonde", "brown", "gray"
  - `eyeColor`: e.g., "blue", "green", "brown", "hazel"
  - `hobby`: e.g., "chess", "surfing", "painting", "cooking"
  - `favoriteFood`: e.g., "sushi", "tacos", "curry", "pasta"
  - `vehicle`: e.g., "motorcycle", "convertible", "yacht", "helicopter"
  - `distinguishingFeature`: e.g., "scar on left cheek", "gold tooth", "tattoo of a dragon"

## 4. API Specification

### GET /api/cases/{id}/suspects
**Description:** Get the full suspect dossier.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Success Response (200 OK):**
```json
{
  "suspects": [
    {
      "id": "suspect-carmen",
      "name": "Carmen Sandiego",
      "photoKey": "carmen_photo",
      "traits": {
        "hairColor": "black",
        "eyeColor": "brown",
        "hobby": "tango dancing",
        "favoriteFood": "empanadas",
        "vehicle": "convertible",
        "distinguishingFeature": "red trench coat"
      }
    },
    {
      "id": "suspect-vic",
      "name": "Vic the Slick",
      "photoKey": "vic_photo",
      "traits": {
        "hairColor": "blonde",
        "eyeColor": "blue",
        "hobby": "poker",
        "favoriteFood": "steak",
        "vehicle": "motorcycle",
        "distinguishingFeature": "gold tooth"
      }
    }
  ]
}
```

**Error Responses:**
- `404 Not Found` — Case not found.
- `403 Forbidden` — Case does not belong to this session.

### POST /api/cases/{id}/warrant
**Description:** Issue an arrest warrant for a suspect.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Request Body:**
```json
{
  "suspectId": "suspect-carmen"
}
```

**Success Response (200 OK) — Correct Warrant:**
```json
{
  "result": "won",
  "message": "Congratulations! You've apprehended Carmen Sandiego!",
  "warrant": {
    "suspectId": "suspect-carmen",
    "suspectName": "Carmen Sandiego",
    "cityId": "cairo",
    "cityName": "Cairo",
    "issuedAt": "2025-01-15T15:00:00Z"
  },
  "caseStatus": "won"
}
```

**Success Response (200 OK) — Wrong Suspect:**
```json
{
  "result": "lost",
  "reason": "wrong_suspect",
  "message": "That's not the right suspect. Carmen Sandiego got away!",
  "warrant": {
    "suspectId": "suspect-vic",
    "suspectName": "Vic the Slick",
    "cityId": "cairo",
    "cityName": "Cairo",
    "issuedAt": "2025-01-15T15:00:00Z"
  },
  "correctSuspect": {
    "id": "suspect-carmen",
    "name": "Carmen Sandiego"
  },
  "caseStatus": "lost"
}
```

**Success Response (200 OK) — Wrong City:**
```json
{
  "result": "lost",
  "reason": "wrong_city",
  "message": "You issued the warrant in the wrong city. The suspect escaped!",
  "warrant": {
    "suspectId": "suspect-carmen",
    "suspectName": "Carmen Sandiego",
    "cityId": "tokyo",
    "cityName": "Tokyo",
    "issuedAt": "2025-01-15T15:00:00Z"
  },
  "caseStatus": "lost"
}
```

**Error Responses:**
- `400 Bad Request` — Missing or invalid `suspectId`.
  ```json
  { "error": "Suspect not found", "code": "INVALID_SUSPECT" }
  ```
- `409 Conflict` — Warrant already issued or case completed.
  ```json
  { "error": "Warrant already issued for this case", "code": "WARRANT_ALREADY_ISSUED" }
  ```
- `404 Not Found` — Case not found.

## 5. Data Models

### Suspect
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique (e.g., `suspect-carmen`) |
| `name` | string | Required, display name |
| `photoKey` | string | Required, references a UI asset |
| `traits` | SuspectTraits | Required |

### SuspectTraits
| Field | Type | Constraints |
|---|---|---|
| `hairColor` | string | Required, from predefined set |
| `eyeColor` | string | Required, from predefined set |
| `hobby` | string | Required |
| `favoriteFood` | string | Required |
| `vehicle` | string | Required |
| `distinguishingFeature` | string | Required |

### Warrant
| Field | Type | Constraints |
|---|---|---|
| `suspectId` | string | References Suspect.id |
| `suspectName` | string | Denormalized for display |
| `cityId` | string | The city where warrant was issued |
| `cityName` | string | Denormalized for display |
| `issuedAt` | datetime | UTC |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-SD-1 | Warrant already issued for this case | HTTP 409 with `{ "error": "Warrant already issued for this case", "code": "WARRANT_ALREADY_ISSUED" }` |
| EC-SD-2 | Wrong suspect selected | Case status becomes `lost`; response includes correct suspect name |
| EC-SD-3 | Correct suspect but wrong city | Case status becomes `lost`; reason is `wrong_city` |
| EC-SD-4 | Warrant issued when case is already lost (0 steps) | HTTP 409 with `{ "error": "Case is already completed", "code": "CASE_COMPLETED" }` |
| EC-SD-5 | Invalid suspect ID | HTTP 400 with `{ "error": "Suspect not found", "code": "INVALID_SUSPECT" }` |
| EC-SD-6 | Suspect ID missing from request body | HTTP 400 with `{ "error": "Suspect ID is required", "code": "MISSING_SUSPECT_ID" }` |
| EC-SD-7 | View dossier when case is completed | Allowed — dossier is read-only and always accessible during and after a case |
| EC-SD-8 | Warrant issued before visiting any cities (from first city) | Allowed — warrant can be issued at any time; will likely result in `wrong_city` loss |

## 7. Acceptance Criteria

1. **Given** an active case, **when** GET /api/cases/{id}/suspects is called, **then** a list of 10+ suspects with complete traits is returned.
2. **Given** an active case with no warrant issued, **when** POST /api/cases/{id}/warrant is called with a valid suspect ID, **then** the warrant is recorded and the case outcome is evaluated.
3. **Given** the correct suspect AND the player is in the final trail city, **when** a warrant is issued, **then** the case status becomes `won` and a success message is returned.
4. **Given** the wrong suspect, **when** a warrant is issued, **then** the case status becomes `lost` with reason `wrong_suspect` and the correct suspect is revealed.
5. **Given** the correct suspect but the player is NOT in the final trail city, **when** a warrant is issued, **then** the case status becomes `lost` with reason `wrong_city`.
6. **Given** a warrant has already been issued, **when** POST /api/cases/{id}/warrant is called again, **then** HTTP 409 is returned.
7. **Given** a completed case, **when** GET /api/cases/{id}/suspects is called, **then** the dossier is still returned (read-only access).
8. **Given** an invalid suspect ID, **when** a warrant is issued, **then** HTTP 400 is returned.
9. **Given** the suspect dossier, **when** inspected, **then** each suspect has all 6 trait categories populated with non-empty values.
10. **Given** a warrant is issued, **when** the response is returned, **then** it includes the suspect name, city name, and timestamp.
