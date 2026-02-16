# FRD: NPC Conversations

| Field | Value |
|---|---|
| **Feature** | NPC Conversations |
| **PRD Refs** | FR-3.1, FR-3.2, FR-3.3, FR-3.4, FR-3.5 |
| **Dependencies** | frd-case-system, frd-city-travel |
| **Status** | Approved |

## 1. Overview

NPC Conversations enables the player to chat with 2–3 non-player characters in each city. Dialogue is generated at runtime by Azure AI Foundry, with NPCs providing clues about the suspect's traits (hair color, eye color, hobby, food, vehicle, feature). The system enforces a 280-character input limit, maintains a 20-message history cap per NPC per case, sanitizes prompt injection attempts, and falls back to static responses when AI is unavailable.

## 2. User Stories

### US-NC-1: Talk to an NPC
- **As a** player, **I want** to type a question or statement to an NPC, **so that** I receive clues about the suspect.
- **Acceptance Criteria:**
  - The player can send a text message (1–280 characters) to a specific NPC.
  - The NPC responds with an AI-generated message containing contextual clues.
  - Messages are displayed in a chat-like conversation format.

### US-NC-2: View Conversation History
- **As a** player, **I want** to see my previous messages with an NPC, **so that** I can review clues I've already received.
- **Acceptance Criteria:**
  - The chat history for each NPC is maintained for the duration of the case.
  - History includes both player and NPC messages with timestamps.
  - Maximum 20 messages (10 player + 10 NPC) are retained per NPC.

### US-NC-3: AI Fallback
- **As a** player, **I want** to still receive some response when AI is unavailable, **so that** my gameplay is not blocked.
- **Acceptance Criteria:**
  - If AI times out (>5 seconds) or is unreachable, a static fallback response is returned.
  - The fallback response is contextually plausible (generic NPC dialogue).
  - The player is not informed that AI failed — the experience remains seamless.

### US-NC-4: Input Sanitization
- **As a** system, **I want** to sanitize player input against prompt injection, **so that** the AI system prompt is not compromised.
- **Acceptance Criteria:**
  - Known injection patterns are detected and neutralized before sending to AI.
  - The player receives a generic NPC response if injection is detected.
  - The injection attempt is logged server-side.

## 3. Functional Requirements

### FRD-NC-1: NPC Data
- Each NPC SHALL have: `id`, `name`, `role`, `personality`, `cityId`, and `clueTraits` (array of suspect trait keys this NPC can hint at).
- NPC data SHALL be defined in `data/npcs.json`.
- Each city SHALL have 2–3 NPCs assigned.

### FRD-NC-2: Chat Message Processing
- POST /api/cases/{id}/npcs/{npcId}/chat SHALL:
  1. Validate the case is `active`.
  2. Validate the NPC belongs to the player's current city.
  3. Validate the message is 1–280 characters.
  4. Sanitize the message for prompt injection.
  5. Retrieve the chat history for this NPC in this case.
  6. Construct the AI prompt with system context, NPC personality, suspect clue traits, and chat history.
  7. Call Azure AI Foundry with a 5-second timeout.
  8. If AI succeeds, return the AI response.
  9. If AI fails or times out, return a static fallback response.
  10. Append both messages (player + NPC) to the chat history.

### FRD-NC-3: AI Prompt Construction
- The system prompt SHALL include:
  - NPC name, role, and personality description.
  - The suspect traits this NPC can reveal (e.g., "The suspect has red hair").
  - Instructions to stay in character and not reveal being an AI.
  - Instructions to weave clues naturally into conversation.
  - Instructions to limit response length to 500 characters.
- The user messages in the prompt SHALL include the chat history (up to 20 messages).

### FRD-NC-4: Chat History Cap
- Maximum 20 messages per NPC per case (10 player messages + 10 NPC responses).
- When the cap is reached, the API SHALL return HTTP 429 with a message indicating the NPC has no more to say.
- The NPC's final message SHALL be a farewell-style response (e.g., "I've told you everything I know, detective.").

### FRD-NC-5: AI Timeout and Fallback
- The AI call SHALL have a timeout of 5 seconds.
- If the AI call times out or returns an error, the system SHALL select a random static fallback response from a pool of 5+ generic responses defined in `data/fallback-responses.json`.
- Fallback responses SHALL be personality-neutral but contextually appropriate (e.g., "Hmm, I'm not sure about that. Try asking someone else around here.").
- The fallback response SHALL be logged with reason (timeout, error, etc.).

### FRD-NC-6: Prompt Injection Sanitization
- The system SHALL check player input against a blocklist of known injection patterns:
  - "ignore previous instructions"
  - "you are now"
  - "system prompt"
  - "forget your instructions"
  - Regex patterns for role reassignment attempts.
- If an injection is detected:
  - The input SHALL NOT be sent to the AI.
  - A generic NPC response SHALL be returned (e.g., "I don't understand what you mean.").
  - The attempt SHALL be logged with the original input (sanitized for logging).

### FRD-NC-7: NPC Scope Validation
- The player SHALL only be able to chat with NPCs in their current city.
- Attempting to chat with an NPC in a different city SHALL return HTTP 400.

## 4. API Specification

### POST /api/cases/{id}/npcs/{npcId}/chat
**Description:** Send a message to an NPC and receive a response.

**Request Headers:**
- `X-Session-Id: {sessionId}` (required)

**Request Body:**
```json
{
  "message": "Have you seen anyone suspicious around here?"
}
```

**Success Response (200 OK):**
```json
{
  "npcMessage": {
    "id": "msg-789",
    "npcId": "npc-somchai",
    "npcName": "Somchai",
    "text": "Ah yes, I saw someone at the market yesterday. They had bright red hair and were asking about trains heading west. They seemed to love spicy food — kept buying chili paste!",
    "timestamp": "2025-01-15T14:35:00Z"
  },
  "chatHistory": {
    "messageCount": 4,
    "maxMessages": 20,
    "remainingMessages": 16
  }
}
```

**Error Responses:**
- `400 Bad Request` — Message empty, exceeds 280 chars, or NPC not in current city.
  ```json
  { "error": "Message exceeds 280 character limit", "code": "MESSAGE_TOO_LONG" }
  ```
  ```json
  { "error": "NPC is not in your current city", "code": "NPC_WRONG_CITY" }
  ```
- `409 Conflict` — Case not active.
- `404 Not Found` — Case or NPC not found.
- `429 Too Many Requests` — Chat history cap reached.
  ```json
  { "error": "Conversation limit reached with this NPC", "code": "CHAT_CAP_REACHED" }
  ```

## 5. Data Models

### NPC
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique (e.g., `npc-somchai`) |
| `name` | string | Required, display name |
| `role` | string | Required (e.g., "Market Vendor") |
| `personality` | string | Required, 1–2 sentence personality description for AI prompt |
| `cityId` | string | Required, references City.id |
| `clueTraits` | string[] | 1–3 suspect trait keys (e.g., `["hairColor", "food"]`) |

### NpcMessage
| Field | Type | Constraints |
|---|---|---|
| `id` | string | Unique, generated |
| `npcId` | string | References NPC.id |
| `sender` | enum | `player` or `npc` |
| `text` | string | 1–500 chars (player max 280, NPC max 500) |
| `timestamp` | datetime | UTC |

### ChatHistory
| Field | Type | Constraints |
|---|---|---|
| `caseId` | string | References Case.id |
| `npcId` | string | References NPC.id |
| `messages` | NpcMessage[] | Max 20 messages |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-NC-1 | AI times out (>5s) | Return static fallback response; log timeout event |
| EC-NC-2 | AI service unavailable (HTTP 5xx) | Return static fallback response; log service error |
| EC-NC-3 | Empty message (0 characters) | HTTP 400 with `{ "error": "Message cannot be empty", "code": "EMPTY_MESSAGE" }` |
| EC-NC-4 | Message exceeds 280 characters | HTTP 400 with `{ "error": "Message exceeds 280 character limit", "code": "MESSAGE_TOO_LONG" }` |
| EC-NC-5 | Prompt injection attempt detected | Return generic NPC response; do not send to AI; log the attempt |
| EC-NC-6 | Chat history cap reached (20 messages) | HTTP 429 with farewell message and `{ "code": "CHAT_CAP_REACHED" }` |
| EC-NC-7 | NPC not in player's current city | HTTP 400 with `{ "error": "NPC is not in your current city", "code": "NPC_WRONG_CITY" }` |
| EC-NC-8 | NPC ID does not exist | HTTP 404 with `{ "error": "NPC not found", "code": "NPC_NOT_FOUND" }` |
| EC-NC-9 | Chat with NPC when case is completed | HTTP 409 with `{ "error": "Case is already completed", "code": "CASE_COMPLETED" }` |
| EC-NC-10 | AI returns empty response | Treat as AI failure; use fallback response |
| EC-NC-11 | AI response exceeds 500 characters | Truncate at 500 characters |

## 7. Acceptance Criteria

1. **Given** an active case and a valid NPC in the current city, **when** the player sends a 1–280 character message, **then** the NPC responds with an AI-generated clue-containing message.
2. **Given** the AI service times out after 5 seconds, **when** the player sends a message, **then** a static fallback response is returned and the timeout is logged.
3. **Given** the AI service is unavailable, **when** the player sends a message, **then** a static fallback response is returned seamlessly.
4. **Given** a chat history with 20 messages, **when** the player sends another message, **then** HTTP 429 is returned with a farewell message.
5. **Given** a player input containing "ignore previous instructions," **when** processed, **then** the input is not sent to AI, a generic response is returned, and the attempt is logged.
6. **Given** a message exceeding 280 characters, **when** sent, **then** HTTP 400 is returned with code `MESSAGE_TOO_LONG`.
7. **Given** an NPC in a different city, **when** the player tries to chat with that NPC, **then** HTTP 400 is returned with code `NPC_WRONG_CITY`.
8. **Given** a completed case, **when** the player tries to chat, **then** HTTP 409 is returned.
9. **Given** a valid conversation, **when** chat history is retrieved, **then** all messages (player and NPC) are returned in chronological order with timestamps.
10. **Given** the AI generates a response, **when** the response exceeds 500 characters, **then** it is truncated to 500 characters.
