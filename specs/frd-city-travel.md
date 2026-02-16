# FRD: City & Travel

| Field | Value |
|---|---|
| **Feature** | City & Travel |
| **PRD Refs** | FR-2.1, FR-2.2, FR-2.3, FR-2.4 |
| **Dependencies** | frd-case-system |
| **Status** | Draft |

## 1. Overview

The City & Travel system manages the player's movement between cities along the case trail. Each city has a visual backdrop and 2–3 NPCs. Travel is presented as a text-based selection of 3 destination options (1 correct next city on the trail, 2 decoys). Each travel action consumes one step, and reaching 0 steps without solving the case triggers a loss.

## 2. User Stories

### US-CT-1: View Current City
- **As a** player, **I want** to see my current city's name, description, and available NPCs, **so that** I can investigate for clues.
- **Acceptance Criteria:**
  - The city name, region, description, and background image key are returned.
  - A list of 2–3 NPCs with their names and roles is included.

### US-CT-2: Travel to Another City
- **As a** player, **I want** to choose from 3 destination options, **so that** I can follow the suspect's trail.
- **Acceptance Criteria:**
  - Exactly 3 travel options are presented: 1 correct (next trail city) and 2 decoys.
  - Selecting a destination decrements remaining steps by 1.
  - The player's current city updates to the selected destination.
  - The visited cities list is updated.

### US-CT-3: Dead End on Wrong Travel
- **As a** player, **I want** to know when I've traveled to a decoy city, **so that** I can adjust my strategy.
- **Acceptance Criteria:**
  - Decoy cities still have NPCs and can be investigated.
  - The player can travel again from a decoy city (back to the correct path if offered).
  - No explicit "wrong city" message is shown — the player must deduce from NPC clues.

### US-CT-4: Step Exhaustion
- **As a** player, **I want** to be notified when I run out of steps, **so that** I know the case is lost.
- **Acceptance Criteria:**
  - When remaining steps reach 0 and no correct warrant is issued, the case transitions to `lost`.
  - The API prevents further travel actions.

## 3. Functional Requirements

### FRD-CT-1: City Database
- The system SHALL define at least 15 cities across 5 continents in `data/cities.json`.
- Each city SHALL have: `id`, `name`, `region`, `continent`, `description`, `backgroundKey`, and `npcIds` (2–3 NPC references).
- Continents represented: North America, South America, Europe, Asia, Africa (minimum 3 cities each).

### FRD-CT-2: Current City Retrieval
- GET /api/cases/{id}/city SHALL return the current city's details including NPCs and available travel options.
- Travel options SHALL only be included when the case status is `active` and `remainingSteps` > 0.

### FRD-CT-3: Travel Option Generation
- When the player is on a trail city, the system SHALL generate exactly 3 travel options:
  - 1 correct option: the next city in the trail sequence.
  - 2 decoy options: selected from the case config's `decoyTravel` mapping for the current city.
- The order of the 3 options SHALL be randomized on each request.
- When the player is on a decoy city (not on the trail), the system SHALL offer 3 options:
  - The correct next trail city the player should have gone to.
  - 2 other decoy cities.

### FRD-CT-4: Travel Execution
- POST /api/cases/{id}/travel SHALL:
  1. Validate the case is `active` and `remainingSteps` > 0.
  2. Validate the selected `cityId` is one of the 3 offered travel options.
  3. Decrement `remainingSteps` by 1.
  4. Update `currentCityIndex` if the player traveled to the correct next trail city.
  5. Append the new city to `visitedCities`.
  6. If `remainingSteps` is now 0 and the player has not won, set case status to `lost`.
  7. Return the new city state.

### FRD-CT-5: Final City Behavior
- When the player reaches the final city in the trail, travel options SHALL NOT be offered.
- The city response SHALL indicate `isFinalCity: true`.
- The player can still investigate NPCs and issue a warrant from the final city.

### FRD-CT-6: Travel Restrictions
- Travel SHALL be rejected with HTTP 409 if:
  - The case is not `active`.
  - `remainingSteps` is 0.
- Travel SHALL be rejected with HTTP 400 if:
  - The selected city is the current city.
  - The selected city is not in the offered travel options.

## 4. API Specification

### GET /api/cases/{id}/city
**Description:** Get the current city information, NPCs, and travel options.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Success Response (200 OK):**
```json
{
  "city": {
    "id": "bangkok",
    "name": "Bangkok",
    "region": "Southeast Asia",
    "continent": "Asia",
    "description": "The bustling capital of Thailand, known for ornate shrines and vibrant street life.",
    "backgroundKey": "bangkok_bg"
  },
  "npcs": [
    { "id": "npc-somchai", "name": "Somchai", "role": "Market Vendor" },
    { "id": "npc-mae", "name": "Mae", "role": "Tuk-tuk Driver" }
  ],
  "travelOptions": [
    { "cityId": "tokyo", "cityName": "Tokyo", "description": "Fly east to Japan's neon-lit capital" },
    { "cityId": "mumbai", "cityName": "Mumbai", "description": "Head west to India's city of dreams" },
    { "cityId": "sydney", "cityName": "Sydney", "description": "Journey south to Australia's harbor city" }
  ],
  "remainingSteps": 9,
  "isFinalCity": false
}
```

**Error Responses:**
- `404 Not Found` — Case not found.
- `403 Forbidden` — Case does not belong to this session.

### POST /api/cases/{id}/travel
**Description:** Travel to a selected city.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Request Body:**
```json
{
  "cityId": "tokyo"
}
```

**Success Response (200 OK):**
```json
{
  "city": {
    "id": "tokyo",
    "name": "Tokyo",
    "region": "East Asia",
    "continent": "Asia",
    "description": "A dazzling metropolis blending ultramodern and traditional.",
    "backgroundKey": "tokyo_bg"
  },
  "npcs": [
    { "id": "npc-yuki", "name": "Yuki", "role": "Shrine Keeper" },
    { "id": "npc-kenji", "name": "Kenji", "role": "Street Food Chef" },
    { "id": "npc-hana", "name": "Hana", "role": "Train Conductor" }
  ],
  "remainingSteps": 8,
  "caseStatus": "active",
  "isFinalCity": false
}
```

**Error Responses:**
- `400 Bad Request` — Invalid city ID or city not in travel options.
- `409 Conflict` — Case not active or no remaining steps.
- `404 Not Found` — Case not found.

## 5. Data Models

### City
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique, kebab-case (e.g., `new-york`) |
| `name` | string | Required, display name |
| `region` | string | Required (e.g., "Southeast Asia") |
| `continent` | string | One of: `North America`, `South America`, `Europe`, `Asia`, `Africa` |
| `description` | string | Required, 1–2 sentences |
| `backgroundKey` | string | Required, references a UI background asset |
| `npcIds` | string[] | 2–3 NPC IDs |

### TravelOption
| Field | Type | Constraints |
|---|---|---|
| `cityId` | string | Must be a valid City.id |
| `cityName` | string | Display name of the destination |
| `description` | string | 1-sentence flavor text for the travel option |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-CT-1 | Travel when `remainingSteps` is 0 | HTTP 409 with `{ "error": "No remaining steps", "code": "NO_STEPS" }` |
| EC-CT-2 | Travel to the current city | HTTP 400 with `{ "error": "Already in this city", "code": "SAME_CITY" }` |
| EC-CT-3 | Travel to a city not in the 3 options | HTTP 400 with `{ "error": "Invalid travel destination", "code": "INVALID_DESTINATION" }` |
| EC-CT-4 | Travel when case is completed | HTTP 409 with `{ "error": "Case is already completed", "code": "CASE_COMPLETED" }` |
| EC-CT-5 | City data missing for a trail city | Server logs error; returns HTTP 500. Case initialization should validate all trail cities exist. |
| EC-CT-6 | Request travel options at final city | `travelOptions` is an empty array; `isFinalCity` is true |
| EC-CT-7 | City has fewer than 2 NPCs in config | Server logs warning; returns available NPCs (minimum 0) |

## 7. Acceptance Criteria

1. **Given** an active case, **when** GET /api/cases/{id}/city is called, **then** the current city details, 2–3 NPCs, and exactly 3 travel options are returned.
2. **Given** 3 travel options, **when** the player selects a valid destination, **then** remaining steps decrease by 1 and the current city updates.
3. **Given** 3 travel options, **when** the player selects a city not in the options, **then** HTTP 400 is returned.
4. **Given** remaining steps is 1, **when** the player travels and has not issued a correct warrant, **then** remaining steps becomes 0 and case status becomes `lost`.
5. **Given** the player is at the final trail city, **when** GET /api/cases/{id}/city is called, **then** `travelOptions` is empty and `isFinalCity` is true.
6. **Given** the case is completed, **when** POST /api/cases/{id}/travel is called, **then** HTTP 409 is returned.
7. **Given** the player travels to a decoy city, **when** GET /api/cases/{id}/city is called from the decoy, **then** 3 new travel options are provided (including the correct trail city).
8. **Given** 15+ cities are configured, **when** cities are inspected, **then** at least 3 cities exist on each of 5 continents.
