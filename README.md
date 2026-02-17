# spec2cloud Shell: Next.js + .NET

## What is this?

A spec2cloud shell template for building full-stack applications with:
- **Frontend**: Next.js 16 (TypeScript, App Router, Tailwind CSS)
- **Backend**: .NET 10 (Minimal API)
- **Orchestration**: .NET Aspire (local dev)
- **Deployment**: Azure Container Apps via Azure Developer CLI (azd)
- **Testing**: Playwright (e2e) + Cucumber.js (Gherkin/BDD) + xUnit (.NET)

spec2cloud is an AI-driven development workflow. You write a PRD, and spec2cloud agents handle the rest — reviewing specs, generating tests, implementing features, and deploying to Azure.

## Prerequisites

- Node.js 20+
- .NET 10 SDK
- Docker (for containerized deployment)
- Azure Developer CLI (azd) — for Azure deployment
- GitHub Copilot CLI or compatible AI coding agent

## Quick Start

### 1. Clone this template
```bash
git clone <repo-url> my-app
cd my-app
```

### 2. Install dependencies
```bash
npm install
cd src/web && npm install && cd ../..
cd src/api && dotnet restore && cd ../..
```

### 3. Run locally with Aspire
```bash
npm run aspire
```
This starts both the API and web frontend via .NET Aspire. The Aspire dashboard shows all services at https://localhost:15888.

Or run services individually:
```bash
# API only (.NET)
cd src/api && dotnet run

# Web only (Next.js)
npm run dev
```

### 4. Write your PRD
Edit `specs/prd.md` with your product requirements. See the spec2cloud workflow below for what happens next.

## How spec2cloud Works

spec2cloud uses an AI orchestrator (defined in `AGENTS.md`) that drives your project through 6 phases. Each phase has a specialized agent and most have human approval gates.

### The 6 Phases

| Phase | Name | Agent | Human Gate | What Happens |
|-------|------|-------|------------|-------------|
| 0 | Shell Setup | Orchestrator | ✅ | Verify shell structure, scaffold directories |
| 1 | Spec Refinement | `agents/spec-refinement.md` | ✅ | Review PRD for completeness, break into FRDs |
| 2 | Gherkin Generation | `agents/gherkin-generation.md` | ✅ | Convert each FRD into Gherkin feature files |
| 3 | Test Scaffolding | `agents/test-generation.md` | ❌ | Generate test code from Gherkin (red baseline) |
| 4 | Implementation | `agents/implementation.md` | ✅ | Write code to make all tests pass (TDD) |
| 5 | Deployment | `agents/deploy.md` | ✅ | Provision Azure resources, deploy, smoke test |

### Using the Workflow

1. **Write your PRD** in `specs/prd.md`
2. **Start the orchestrator** — open this project in GitHub Copilot (or compatible AI agent) and it will read `AGENTS.md` to begin orchestration
3. **Approve at gates** — the agent pauses at human gates for your review
4. **Watch it build** — the agent generates specs → tests → code → deployment

### State Management

Progress is tracked in `.spec2cloud/state.json` and `.spec2cloud/audit.log`. The orchestrator reads state on startup and resumes where it left off. You can restart the AI session at any time — it will pick up from the last checkpoint.

## Project Structure

```
├── AGENTS.md                    # Orchestrator instructions (6-phase workflow)
├── agents/                      # Specialized agent prompts
│   ├── spec-refinement.md       # Phase 1: PRD/FRD review
│   ├── gherkin-generation.md    # Phase 2: FRD → Gherkin
│   ├── test-generation.md       # Phase 3: Gherkin → tests
│   ├── implementation.md        # Phase 4: TDD implementation
│   └── deploy.md                # Phase 5: Azure deployment
├── specs/                       # Your product specifications
│   ├── prd.md                   # Product Requirements Document (start here!)
│   └── features/                # Generated Gherkin .feature files
├── src/
│   ├── api/                     # .NET 10 Minimal API
│   │   ├── Program.cs           # API entry point
│   │   ├── Endpoints/           # API route handlers
│   │   ├── Models/              # Data models
│   │   ├── Services/            # Business logic interfaces & implementations
│   │   └── tests/               # xUnit tests
│   ├── web/                     # Next.js 16 frontend
│   │   └── src/app/             # App Router pages & components
│   ├── AppHost/                 # .NET Aspire orchestrator
│   └── ServiceDefaults/         # Shared service configuration
├── e2e/                         # Playwright end-to-end tests
├── tests/                       # Cucumber.js BDD tests
├── infra/                       # Azure infrastructure (Bicep)
├── azure.yaml                   # Azure Developer CLI config
├── .spec2cloud/                 # Orchestration state
│   ├── state.json               # Current phase & progress
│   └── audit.log                # Action history
└── .github/                     # CI/CD workflows
```

## Available Scripts

| Script | Command | Description |
|--------|---------|-------------|
| Dev | `npm run dev` | Start Next.js dev server only |
| Aspire | `npm run aspire` | Start all services via .NET Aspire |
| Build | `npm run build` | Build the Next.js frontend |
| E2E Tests | `npm run test:e2e` | Run Playwright end-to-end tests |
| BDD Tests | `npm run test:cucumber` | Run Cucumber.js Gherkin tests |
| All Tests | `npm run test:all` | Run all test suites |

## Optional: Power NPC dialog with GitHub Models

NPC chat can call GitHub Models inference when enabled in the API configuration.

1. Set a token with `models:read` scope in one of: `COPILOT_GITHUB_TOKEN`, `GH_TOKEN`, or `GITHUB_TOKEN`.
2. Enable with environment variable `GitHubModels__Enabled=true` (or set `GitHubModels:Enabled` in config).
3. Optionally override `GitHubModels__Model` and `GitHubModels__Endpoint`.
4. If no token environment variable is set, the API also attempts `gh auth token` as a fallback.

If disabled or if the model request fails, the API automatically falls back to built-in NPC responses.

## Testing Strategy

spec2cloud generates a 4-layer test pyramid:

1. **xUnit** (`.NET unit tests`) — individual service/handler methods
2. **Cucumber.js** (`tests/features/`) — BDD scenarios from Gherkin specs
3. **Playwright** (`e2e/`) — full user journey end-to-end tests
4. **Reqnroll** (`.NET integration`) — backend API integration tests

Tests are generated in Phase 3 as a **red baseline** (they compile but fail). Phase 4 implements code to make them pass.

## Deploy to Azure

```bash
# Login to Azure
azd auth login

# Provision infrastructure and deploy
azd up
```

This creates Azure Container Apps for the API and web frontend, plus supporting infrastructure (Container Registry, monitoring) defined in `infra/`.

## Customizing the Shell

This template ships with a chat-based UI pattern. You can:

- **Replace the frontend** — swap Next.js for any framework
- **Replace the backend** — swap .NET for any language
- **Add services** — add databases, caches, or other backends in `AppHost.cs`
- **Modify agents** — customize the agent prompts in `agents/` for your workflow
- **Change models** — update `.spec2cloud/models.json` to use different AI models

The key files to customize are `AGENTS.md` (orchestration behavior) and the agent files in `agents/`.

## License

ISC
