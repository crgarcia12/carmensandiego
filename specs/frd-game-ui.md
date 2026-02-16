# FRD: Game User Interface

| Field | Value |
|---|---|
| **Feature** | Game User Interface |
| **PRD Refs** | FR-5.1, FR-5.2, FR-5.3, FR-5.4, FR-5.5 |
| **Dependencies** | frd-case-system, frd-city-travel, frd-npc-conversations, frd-suspect-dossier, frd-session-management |
| **Status** | Approved |

## 1. Overview

The Game UI is a single-page Next.js application with a retro/pixel-art aesthetic. It provides five main screens (Briefing, City/Investigation, Dossier, Arrest, Summary) with responsive layouts supporting desktop (1024px+) and mobile (375px+). The UI meets WCAG AA accessibility standards and supports full keyboard navigation.

## 2. User Stories

### US-UI-1: View Briefing Screen
- **As a** player, **I want** to see a briefing when my case starts, **so that** I understand what was stolen and begin my investigation.
- **Acceptance Criteria:**
  - The briefing screen displays the case title, stolen treasure name and description, and a narrative briefing text.
  - A "Start Investigation" button navigates to the first city.
  - The screen is displayed immediately after a new case is created.

### US-UI-2: Investigate in a City
- **As a** player, **I want** to see the city background, talk to NPCs, and choose my next destination, **so that** I can gather clues and progress through the case.
- **Acceptance Criteria:**
  - The city screen shows the city name, background image, NPC panel, travel panel, and step counter.
  - NPCs are listed with names and roles; clicking an NPC opens a chat panel.
  - Travel options are listed as clickable destinations with flavor text.
  - The step counter shows remaining steps (e.g., "Steps: 7/10").

### US-UI-3: Chat with NPCs
- **As a** player, **I want** to type messages and see NPC responses in a chat interface, **so that** I can ask for clues.
- **Acceptance Criteria:**
  - Chat panel displays messages in speech-bubble format (player on right, NPC on left).
  - A text input field with a 280-character counter and send button.
  - NPC responses appear with a brief typing indicator (500ms delay).
  - Chat history scrolls to the latest message.

### US-UI-4: View Suspect Dossier
- **As a** player, **I want** to browse suspect cards at any time, **so that** I can compare clues to suspect traits.
- **Acceptance Criteria:**
  - Dossier is accessible via a persistent "Dossier" button/tab visible on the city screen.
  - Each suspect is displayed as a card with name, photo, and all 6 traits.
  - Cards are arranged in a scrollable grid (2–3 columns desktop, 1 column mobile).
  - An "Issue Warrant" button is available on each suspect card.

### US-UI-5: Issue Arrest Warrant
- **As a** player, **I want** to confirm my warrant choice via a dialog, **so that** I don't accidentally arrest the wrong suspect.
- **Acceptance Criteria:**
  - Clicking "Issue Warrant" opens a confirmation dialog with the suspect's name and photo.
  - Dialog text: "Are you sure you want to issue a warrant for [Name]? This action is irreversible."
  - "Confirm" and "Cancel" buttons. Confirm submits the warrant; Cancel closes the dialog.
  - After confirmation, the arrest result screen is shown.

### US-UI-6: View Arrest Result
- **As a** player, **I want** to see whether I caught the right suspect, **so that** I know the outcome of my decision.
- **Acceptance Criteria:**
  - Win: celebratory animation/visual, success message, suspect name.
  - Lose (wrong suspect): failure message, correct suspect revealed.
  - Lose (wrong city): failure message indicating wrong location.
  - A "View Summary" button navigates to the summary screen.

### US-UI-7: View Case Summary
- **As a** player, **I want** to see a recap of my entire case, **so that** I can review my investigation path.
- **Acceptance Criteria:**
  - Summary shows: outcome (won/lost), cities visited (in order), steps used out of total, stolen treasure, correct suspect, player's warrant details.
  - A "Play Again" button starts a new case.

### US-UI-8: Responsive Layout
- **As a** player on mobile, **I want** the game to be playable on my phone, **so that** I can play anywhere.
- **Acceptance Criteria:**
  - Minimum supported width: 375px (iPhone SE).
  - Desktop layout: side panels for NPC chat and travel options.
  - Mobile layout: stacked panels with tab navigation (City | Chat | Travel | Dossier).
  - No horizontal scrolling at any supported breakpoint.

### US-UI-9: Keyboard Accessibility
- **As a** player using keyboard navigation, **I want** all interactive elements to be reachable via Tab/Enter, **so that** I can play without a mouse.
- **Acceptance Criteria:**
  - All buttons, links, and inputs are focusable via Tab.
  - Focus indicators are visible (minimum 2px outline).
  - Enter/Space activates focused buttons.
  - Escape closes modals and dialogs.

## 3. Functional Requirements

### FRD-UI-1: Screen Routing
- The app SHALL use Next.js App Router with the following routes:
  - `/` — Landing/new game (creates session, starts case, redirects to briefing)
  - `/briefing` — BriefingScreen
  - `/city` — CityScreen (investigation hub)
  - `/dossier` — DossierScreen (can be overlay or full page)
  - `/arrest` — ArrestScreen (warrant result)
  - `/summary` — SummaryScreen
- Navigation SHALL be SPA-style (no full page reloads).
- Direct URL access to `/city`, `/dossier`, `/arrest`, `/summary` without an active session SHALL redirect to `/`.

### FRD-UI-2: Visual Theme
- The UI SHALL use a retro/pixel-art inspired visual style.
- Color palette: dark backgrounds (#1a1a2e, #16213e), accent colors (#e94560, #f0c040), text (#e0e0e0).
- Font: monospace or pixel-art style web font.
- City backgrounds SHALL be illustrated images (provided as static assets keyed by `backgroundKey`).

### FRD-UI-3: BriefingScreen Component
- SHALL display: case title (h1), stolen treasure name and description, briefing narrative text (typewriter animation, ~30 chars/sec).
- SHALL include a "Start Investigation" button that navigates to `/city`.
- The typewriter animation SHALL be skippable by clicking or pressing any key.

### FRD-UI-4: CityScreen Component
- SHALL display:
  - **CityBackground**: Full-width background image for the current city.
  - **City info overlay**: City name, region, continent.
  - **StepCounter**: "Steps: X/10" badge, visually prominent.
  - **NpcPanel**: List of 2–3 NPCs with name and role. Clickable to open chat.
  - **TravelPanel**: 3 destination options (or empty if final city). Each option shows city name and flavor text.
  - **Dossier button**: Persistent button/tab to open the dossier.
- On mobile, these panels SHALL be arranged in tabs: City | Chat | Travel | Dossier.

### FRD-UI-5: NpcPanel & Chat Interface
- Clicking an NPC SHALL open a chat panel/modal.
- Chat SHALL display messages in a scrollable list with speech bubbles.
- Player messages: right-aligned, blue background.
- NPC messages: left-aligned, dark background with NPC name label.
- Input: text field (placeholder: "Ask a question...") with character counter (X/280) and send button.
- Send on Enter key press (Shift+Enter for newline if multi-line).
- After sending, show a typing indicator (animated dots) for 500ms before displaying the NPC response.
- When chat cap is reached (20 messages), disable the input field and show "This NPC has nothing more to say."

### FRD-UI-6: TravelPanel Component
- SHALL display exactly 3 travel options as clickable cards/buttons.
- Each option shows: destination city name and 1-sentence description.
- Clicking an option SHALL trigger a confirmation: "Travel to [City Name]? This will use 1 step."
- After travel, the city screen SHALL update with the new city's data.
- At the final city, the travel panel SHALL display: "You've reached the end of the trail. Issue a warrant to make your arrest."

### FRD-UI-7: StepCounter Component
- SHALL display remaining steps as "Steps: X/10".
- When steps ≤ 3, the counter SHALL change to a warning color (red/orange).
- When steps reach 0, the counter SHALL display "No steps remaining" and the case transitions to lost.

### FRD-UI-8: DossierScreen Component
- SHALL display all suspects in a card grid.
- **SuspectCard** component SHALL show: photo placeholder (using `photoKey`), suspect name, and a collapsible/expandable traits section.
- Traits section SHALL list all 6 traits with labels (e.g., "Hair: red", "Eyes: blue").
- Each card SHALL have an "Issue Warrant" button.
- The "Issue Warrant" button SHALL open the warrant confirmation dialog.

### FRD-UI-9: ArrestScreen Component
- SHALL display the warrant result:
  - **Won**: Green theme, celebratory message, suspect name and photo.
  - **Lost (wrong suspect)**: Red theme, failure message, correct suspect revealed.
  - **Lost (wrong city)**: Red theme, failure message about wrong location.
- SHALL include a "View Summary" button.

### FRD-UI-10: SummaryScreen Component
- SHALL display: outcome badge (Won/Lost), stolen treasure, cities visited (as a path/timeline), steps used (X/10), correct suspect card, player's warrant details (suspect + city), "Play Again" button.
- "Play Again" SHALL create a new case and navigate to `/briefing`.

### FRD-UI-11: Loading States
- All API calls SHALL show a loading indicator (spinner or skeleton) while in progress.
- Loading indicators SHALL be accessible (aria-live="polite", aria-busy).

### FRD-UI-12: Error States
- API errors SHALL be displayed as dismissible toast notifications.
- Network errors SHALL show a retry banner: "Connection lost. Retrying..." with automatic retry (3 attempts, exponential backoff: 1s, 2s, 4s).
- After 3 failed retries, show a persistent error message: "Unable to connect. Please check your connection and refresh."

### FRD-UI-13: Accessibility (WCAG AA)
- All images SHALL have alt text.
- Color contrast SHALL meet WCAG AA (4.5:1 for normal text, 3:1 for large text).
- Focus indicators SHALL be visible (minimum 2px solid outline).
- All interactive elements SHALL have accessible names (aria-label where needed).
- Screen reader announcements for: city changes, NPC responses, step counter updates, warrant results.

## 4. API Specification

The Game UI is a client that consumes the APIs defined in the other FRDs. No new API endpoints are introduced. The UI interacts with:

| Action | Endpoint | FRD Source |
|---|---|---|
| Create session | POST /api/sessions | frd-session-management |
| Resume session | GET /api/sessions/{id} | frd-session-management |
| Start case | POST /api/cases | frd-case-system |
| Get case state | GET /api/cases/{id} | frd-case-system |
| Get case summary | GET /api/cases/{id}/summary | frd-case-system |
| Get current city | GET /api/cases/{id}/city | frd-city-travel |
| Travel to city | POST /api/cases/{id}/travel | frd-city-travel |
| Chat with NPC | POST /api/cases/{id}/npcs/{npcId}/chat | frd-npc-conversations |
| Get suspects | GET /api/cases/{id}/suspects | frd-suspect-dossier |
| Issue warrant | POST /api/cases/{id}/warrant | frd-suspect-dossier |

### Client-Side Session Management
- On app load, check localStorage for `sessionId`.
- If found, call GET /api/sessions/{id} to validate.
  - If 200: resume session, check for active case.
  - If 410 (expired): clear localStorage, create new session.
  - If 404: clear localStorage, create new session.
- If not found, create new session via POST /api/sessions and store ID in localStorage.
- Include `X-Session-Id` header on all subsequent API calls.

## 5. Data Models

The UI uses the data models defined in the backend FRDs. Client-side TypeScript types:

### Screen State (client-side)
| Field | Type | Description |
|---|---|---|
| `currentScreen` | enum | `briefing`, `city`, `dossier`, `arrest`, `summary` |
| `sessionId` | string | From localStorage |
| `activeCaseId` | string? | Null if no active case |
| `caseState` | CaseState? | Cached case state from API |
| `currentCity` | CityState? | Current city data with NPCs |
| `chatHistories` | Map<npcId, NpcMessage[]> | Client-side chat cache |

## 6. Edge Cases & Error Handling

| # | Edge Case | Expected Behavior |
|---|---|---|
| EC-UI-1 | Page load with no session | Create new session automatically; show loading screen during creation |
| EC-UI-2 | Page load with expired session | Clear localStorage; create new session; redirect to `/` |
| EC-UI-3 | API returns 503 (max sessions) | Show "Server is at capacity. Please try again in a minute." with auto-retry after 60s |
| EC-UI-4 | Network connectivity lost | Show retry banner; auto-retry 3 times with exponential backoff |
| EC-UI-5 | City background image fails to load | Show a solid color fallback background matching the city's continent theme |
| EC-UI-6 | NPC list is empty (0 NPCs in city) | Show message: "There's nobody around to talk to here." |
| EC-UI-7 | No travel options (final city) | Hide travel panel; show warrant prompt instead |
| EC-UI-8 | Browser back button during game | SPA handles navigation; no data loss; state preserved |
| EC-UI-9 | User refreshes page mid-game | Session and case state restored from API via session ID in localStorage |
| EC-UI-10 | Warrant confirmation dialog dismissed | No action taken; player returns to dossier |
| EC-UI-11 | Rapid double-click on travel button | Disable button after first click; prevent duplicate API calls |
| EC-UI-12 | Screen reader user navigates | All screen transitions announced via aria-live region |

## 7. Acceptance Criteria

1. **Given** a new player visits the app, **when** the page loads, **then** a session is created, a case is started, and the briefing screen is displayed.
2. **Given** the briefing screen, **when** "Start Investigation" is clicked, **then** the city screen for the first trail city is displayed with NPCs, travel options, and step counter.
3. **Given** the city screen, **when** an NPC is clicked, **then** a chat panel opens with an input field and the NPC's name displayed.
4. **Given** the chat panel, **when** the player types a message and presses Enter, **then** a typing indicator appears for 500ms followed by the NPC response.
5. **Given** the city screen, **when** a travel option is clicked and confirmed, **then** the step counter decrements and the new city is displayed.
6. **Given** the city screen, **when** the "Dossier" button is clicked, **then** the dossier screen shows all 10+ suspects with their traits.
7. **Given** the dossier screen, **when** "Issue Warrant" is clicked on a suspect, **then** a confirmation dialog appears with the suspect's name.
8. **Given** the confirmation dialog, **when** "Confirm" is clicked, **then** the warrant is submitted and the arrest result screen is displayed.
9. **Given** the arrest result screen, **when** "View Summary" is clicked, **then** the summary screen is displayed with full case recap.
10. **Given** the summary screen, **when** "Play Again" is clicked, **then** a new case is created and the briefing screen is displayed.
11. **Given** a viewport width of 375px, **when** the city screen is displayed, **then** all panels are stacked vertically with tab navigation and no horizontal scrollbar.
12. **Given** keyboard-only navigation, **when** Tab is pressed, **then** all interactive elements receive visible focus in a logical order.
13. **Given** any API call in progress, **when** the response has not arrived, **then** a loading indicator is displayed with aria-busy="true".
14. **Given** an API error, **when** displayed, **then** a dismissible toast notification appears with the error message.
15. **Given** remaining steps ≤ 3, **when** the step counter is displayed, **then** it uses a warning color (red/orange).
