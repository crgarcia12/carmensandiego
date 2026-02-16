# Product Requirements Document (PRD)

| Field          | Value                                      |
|----------------|--------------------------------------------|
| **Product**    | Carmen Sandiego — Web Edition              |
| **Author**     | Carlos Garcia                              |
| **Version**    | 1.0                                        |
| **Date**       | 2026-02-16                                 |
| **Status**     | Approved                                   |

---

## 1. Problem Statement

This project aims to deliver a modernized version of the classic "Where in the World Is Carmen Sandiego?", AI-enhanced web adaptation that preserves the spirit of the original while leveraging modern technology (AI-driven conversations, responsive design, cloud deployment).

---

## 2. Vision

A browser-based detective game where the player is an ACME agent tasked with tracking down Carmen Sandiego's criminal network across the globe. The player gathers clues by chatting with NPCs (powered by AI), travels between cities, and races against time to identify and arrest the thief — all through a visually nostalgic yet modern interface.

---

## 3. Target Audience

- **Primary:** Casual gamers (ages 12+) who enjoy trivia, geography, and detective/puzzle games.
- **Secondary:** Fans of the original "Where in the World Is Carmen Sandiego?" series seeking a nostalgia-driven experience.
- **Tertiary:** Educators looking for an engaging geography learning tool.

**User Persona — "Alex the Explorer":**
- Plays browser games during breaks (5–15 min sessions).
- Enjoys geography trivia and puzzle-solving.
- Expects a responsive, no-install, no-login experience.

---

## 4. Goals & Success Metrics

| Goal                                  | Success Metric                                                        |
|---------------------------------------|-----------------------------------------------------------------------|
| Deliver a playable v1 on the web      | A user can complete a full case (start → clues → arrest) end to end.  |
| Faithful to the original feel         | Visual style evokes the original; core gameplay loop is preserved.    |
| AI-driven NPC interactions            | NPCs respond contextually to player questions via an AI backend.      |
| Responsive across devices             | Playable on desktop (1024px+) and mobile (375px+) without scrolling issues. |
| Fast and reliable                     | Page load < 3s; API response < 2s (p95); 99.5% uptime on Azure.      |

---

## 5. Gameplay Overview

### 5.1 Core Loop

1. **Briefing** — The player receives a case: a villain from Carmen's network has stolen a treasure from a city.
2. **Investigation** — The player travels to cities and talks to NPCs (witnesses, informants) who provide geographical and cultural clues about the thief's next destination.
3. **Travel** — The player chooses the next city to fly to from a list of options. Each travel step advances the clock.
4. **Deduction** — Based on accumulated clues, the player identifies the suspect from a dossier.
5. **Arrest** — If the player is in the correct city with the correct warrant, the thief is arrested. Case solved.

### 5.2 Lose Condition

The player runs out of time (the thief escapes) or issues a warrant for the wrong suspect.

### 5.3 Session Length

A single case should take approximately **2-5 minutes** to complete.

---

## 6. Functional Requirements

### FR-1: Case System

- **FR-1.1**: The system shall generate a case consisting of a stolen treasure, a thief (from a predefined villain roster), and a trail of 4–6 cities.
- **FR-1.2**: Each case shall have a time limit represented as a countdown (number of allowed travel steps, not wall-clock time). The default step limit is **10 steps**.
- **FR-1.3**: Upon case completion (win or lose), the system shall display a summary screen with the outcome, cities visited, and correct answer.
- **FR-1.4**: V1 shall ship with **1 demo case** with a fixed thief and city trail. The data model shall support adding more cases without code changes (data-driven via JSON configuration).
- **FR-1.5**: If no cases are available or case data fails to load, the system shall display an error message: _"Unable to load case data. Please try again later."_

### FR-2: City & Travel

- **FR-2.1**: The game shall include a minimum of **15 real-world cities** across at least 5 continents.
- **FR-2.2**: When in a city, the player shall see a city-themed background image and a list of available NPCs to talk to.
- **FR-2.3**: The player shall choose a destination from a **text-based list** of 3 city options (one correct, two decoys). No interactive/clickable world map in v1.
- **FR-2.4**: Traveling to a new city shall decrement the remaining time/steps by 1.
- **FR-2.5**: If the player has 0 remaining steps, travel options shall be disabled and the lose condition triggered.

### FR-3: NPC Conversations (AI-Powered)

- **FR-3.1**: Each city shall have 2–3 NPCs the player can converse with.
- **FR-3.2**: NPC dialogue shall be generated **at runtime** by Azure AI Foundry. The AI prompt shall incorporate: the thief's next destination, identity traits, and red herrings. Clues are never pre-generated at build time.
- **FR-3.3**: The player shall interact with NPCs via a chat interface (text input, message history). Chat input is limited to **280 characters** per message.
- **FR-3.4**: NPC responses shall be contextual — asking the same NPC different questions yields different relevant answers.
- **FR-3.5**: NPC responses shall return within **2 seconds** (p95).
- **FR-3.6**: If the AI backend is unavailable or times out (>5 seconds), the system shall return a **fallback static response** from a predefined pool of generic clues (e.g., _"I heard the thief was heading somewhere warm..."_). The player experience must not break due to AI downtime.
- **FR-3.7**: All player chat input shall be sanitized before being sent to the AI model to prevent prompt injection attacks. Input containing known injection patterns shall be rejected with a message: _"I don't understand that question. Try asking something else."_
- **FR-3.8**: NPC conversation history shall be limited to the **last 20 messages** per NPC per session to bound memory usage.

### FR-4: Suspect Dossier & Warrants

- **FR-4.1**: The game shall include a dossier of at least **10 suspects** with distinct traits (hair color, hobby, favorite food, vehicle, etc.).
- **FR-4.2**: The player shall be able to review the dossier at any time during the game.
- **FR-4.3**: The player shall be able to issue an arrest warrant for one suspect. This is a **one-time, irreversible** action per case. The UI shall display a confirmation dialog before issuing.
- **FR-4.4**: If the warrant matches the thief and the player is in the correct final city, the arrest succeeds.
- **FR-4.5**: If the warrant is issued for the wrong suspect OR from the wrong city, the case ends immediately as a loss.
- **FR-4.6**: Suspect traits displayed in the dossier shall include: name, hair color, eye color, hobby, favorite food, vehicle, and distinguishing feature.

### FR-5: User Interface

- **FR-5.1**: The UI shall use a pixel-art or retro-inspired visual style reminiscent of the original 1985–1996 editions.
- **FR-5.2**: The main game screen shall display: current city name, city background, NPC chat panel, travel options, and time remaining.
- **FR-5.3**: The UI shall be responsive across desktop (≥1024px) and mobile (≥375px) viewports.
- **FR-5.4**: Navigation between game screens (briefing, city, dossier, arrest, summary) shall not require full page reloads.

### FR-6: Session Management

- **FR-6.1**: Each game session shall be assigned a unique session ID.
- **FR-6.2**: Session state (current city, clues gathered, time remaining, conversation history) shall be persisted server-side.
- **FR-6.3**: If the player closes the browser and returns, they shall be able to resume their session (within a **24-hour window**). After 24 hours, the session expires and is deleted.
- **FR-6.4**: If a session cannot be found or has expired, the system shall redirect the player to a new case start screen with the message: _"Your previous session has expired. Start a new case?"_
- **FR-6.5**: The system shall support a maximum of **1,000 concurrent sessions**. Requests beyond this limit shall receive an HTTP 503 with a retry-after header.

---

## 7. Non-Functional Requirements

| Category        | Requirement                                                                                          |
|-----------------|------------------------------------------------------------------------------------------------------|
| **Performance** | Initial page load (LCP) < 3 seconds on a 4G connection.                                             |
| **Performance** | AI-generated NPC responses < 2 seconds (p95).                                                        |
| **Scalability** | Support at least 100 concurrent sessions without degradation.                                        |
| **Security**    | No user authentication required for v1. Sessions are anonymous.                                      |
| **Security**    | API endpoints shall validate and sanitize all input. No prompt injection from chat input to AI model. |
| **Accessibility**| All interactive elements shall be keyboard-navigable. Text contrast ratio ≥ 4.5:1 (WCAG AA).       |
| **Browser**     | Support latest versions of Chrome, Firefox, Safari, and Edge.                                        |
| **Data**        | No personal data collected. No cookies beyond session ID.                                            |

---

## 8. Technical Constraints

| Constraint               | Detail                                                                 |
|--------------------------|------------------------------------------------------------------------|
| **Frontend**             | Next.js (TypeScript, App Router, Tailwind CSS)                         |
| **Backend**              | .NET 10 Minimal API (C#)                                               |
| **AI Provider**          | Azure AI Foundry (for NPC dialogue generation)                         |
| **Hosting**              | Azure Container Apps via AZD                                           |
| **Infrastructure**       | Azure Bicep (IaC)                                                      |
| **CI/CD**                | GitHub Actions                                                         |
| **Testing**              | xUnit + Reqnroll (.NET), Cucumber.js (Gherkin), Playwright (e2e)       |

---

## 9. Out of Scope (v1)

These are explicitly **not** in the first release:

- **User accounts / login** — sessions are anonymous
- **Multiplayer** — single-player only
- **Leaderboards or scoring** — win/lose only, no points
- **Sound effects or music** — visual-only experience for v1
- **Procedural city generation** — cities are from a curated, static list
- **Mobile app (native)** — web only
- **Localization / i18n** — English only for v1
- **Difficulty levels** — single difficulty for v1
- **Interactive world map** — text-based city selection only for v1

---

## 10. Assumptions

1. Azure AI Foundry (or equivalent Azure OpenAI endpoint) is available and provisioned for NPC dialogue generation.
2. City background images will be sourced from royalty-free/open-license assets or generated.
3. The villain roster and suspect dossier data are maintained as static JSON/configuration, not in a database for v1.
4. The spec2cloud framework is used for the full development lifecycle (PRD → FRD → Gherkin → Tests → Impl → Deploy).

---

## 11. Open Questions (Resolved)

| #  | Question                                                                                      | Resolution |
|----|-----------------------------------------------------------------------------------------------|------------|
| Q1 | Should NPC clues be AI-generated at build time or runtime?                                   | **Runtime.** Clues are generated per-request via Azure AI Foundry. This is the core AI feature — build-time generation would eliminate dynamic conversation. |
| Q2 | How many cases should ship in v1? (1 demo case vs. 5+ replayable cases)                      | **1 demo case.** Data model supports adding more cases via JSON config without code changes. |
| Q3 | Should the world map be interactive (clickable)?                                              | **No.** V1 uses text-based city selection from a list. Interactive map is out of scope for v1. |
| Q4 | Is AI needed to run the game?                                                                 | **Yes, but with fallback.** AI powers NPC conversations (core feature). If AI is unavailable, static fallback responses ensure the game remains playable. |

---

## 12. Revision History

| Version | Date       | Author        | Changes           |
|---------|------------|---------------|--------------------|
| 0.1     | 2026-02-16 | Carlos Garcia | Initial draft      |
| 1.0     | 2026-02-16 | spec2cloud    | Resolved Q1–Q4; added section 3 (Target Audience); added FR-1.4/1.5, FR-2.5, FR-3.6/3.7/3.8, FR-4.5/4.6, FR-6.4/6.5; added acceptance criteria and edge cases; fixed formatting; status → Approved |
