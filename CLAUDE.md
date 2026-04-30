# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Backend
cd Chatbot.Api && dotnet build
cd Chatbot.Api.Tests && dotnet test                    # all tests
cd Chatbot.Api.Tests && dotnet test --filter "FullyQualifiedName~Register_Should_Reject_Weak_Password"  # single test
cd Chatbot.Api.Tests && dotnet test --filter "Category=Workflow"  # by trait

# Frontend
cd Chatbot.Web && npm install && npm run dev            # start dev server (port 15173)
cd Chatbot.Web && npm run build                         # production build

# Dev startup
./start-dev.sh                                          # start both backend + frontend
./stop-dev.sh                                           # stop both

# OpenAPI client generation
npm --prefix Chatbot.Web run generate:api               # from local backend

# EF Migrations (from Chatbot.Api/)
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Key Architecture

### Backend — .NET 9 Minimal API (Chatbot.Api/)

**No controllers.** All routes are `MapGet`/`MapPost`/etc. in `Program.cs` (~5100 lines, single file). Routes are grouped by Swagger tags (Auth, Teams, Members, Projects, Tickets, etc.).

**Domain layer** (`Domain/Entities/`, `Domain/Enums/`): 22 entities, ~19 enums. Key entities: `User`, `Team`, `Member` (with `AIMemberProfile`), `Customer`, `Project`, `Ticket`, `Conversation`, `ConciergeApp`, `AgentWorkflowRun/Step`.

**Infrastructure** (`Infrastructure/Persistence/`): EF Core + SQLite. `AppDbContext` with Fluent API configurations per entity. Migrations in `Infrastructure/Persistence/Migrations/`.

**Services**: `ChatbotAgentRuntime` — wraps Microsoft Agent Framework (OpenAI-compatible). `AgentWorkflowOrchestrator` creates structured multi-step workflows. `AgentWorkflowExecutionService` runs and enriches workflows. `AuditLogService` for audit trail.

**Integrations** (`Integrations/`): External system adapters for ERPNext and Nextcloud with provider interfaces (`ICustomerProvider`, `IProjectProvider`, `ITicketProvider`, `IFileKnowledgeProvider`).

**Configuration**: `ChatbotOptions` bound from `Chatbot:___` config section (ApiKey, Model, Endpoint, SessionLifetimeDays). Validated on startup.

### Frontend — React 19 + Vite 7 + TypeScript (Chatbot.Web/)

Organized by feature with custom hooks:
- `useWorkspace*.ts` hooks per domain (auth, chat, teams, tickets, customers, conversations, workflows, etc.)
- `workspaceApi.ts` — API calls (supplemented by generated OpenAPI client in `src/generated/api/`)
- `AppWorkspace.tsx` — main workspace routing between auth, admin, concierge, customer ops sections

### Tests — xUnit (Chatbot.Api.Tests/)

**TestApiFactory** — `WebApplicationFactory<Program>` with per-test isolated SQLite database. Replaces `IWorkflowTextCompletionService` with `WorkflowTextCompletionStub` to avoid real LLM calls.

**Test pattern**: `IClassFixture<TestApiFactory>`, `_factory.CreateClient()`, HTTP calls via `HttpClient`, assertions on JSON responses. Tests are integration-level — they hit real EF/SQLite through the full middleware stack.

### Data Model Fundamentals

- `EntityBase` — id, timestamps, soft-delete fields
- `ExternalMappedEntityBase` — adds `SourceType`, `ExternalSystemType`, `ExternalId` for project/customer/ticket
- Team is the top-level organizational unit (owned by a User, has Members, Projects, ConciergeApps, IntegrationConnections)
- Member can be Human (linked to User) or AI (with `AIMemberProfile`)
- AgentWorkflowRun → AgentWorkflowStep → AgentExecutionLog forms the workflow execution chain
