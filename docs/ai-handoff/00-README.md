# LaoHR — AI Handoff Package

> **Purpose**: give a new AI agent (with zero prior conversation history) a complete, high-fidelity understanding of the LaoHR project so it can continue development safely without asking the original developer to explain anything.

## How to use this package

1. **Read first**: `31-AI-PROJECT-MEMORY.md` — high-density project memory.
2. **Read second**: `32-CONTINUATION-PACKAGE.md` — designed to be pasted into a fresh LLM as the single continuation prompt.
3. **Read third**: `30-CURRENT-DEVELOPMENT-FRONTIER.md` — where completed work ends and unfinished work begins.
4. **Reference on demand**: the numbered documents below for deep detail on any area.

## Important context

- A prior strategic audit exists in `docs/audit/` (files `00`–`13`). That audit was written **before** the Phase 2–6 work landed. Many items it lists as "missing" are now **implemented** (Projects, Tasks, Milestones, Risks, Issues, Resources, Expenses, Loans, Knowledge, Announcements, Comments, refresh tokens, rate limiting, health checks, Serilog, OpenTelemetry, Docker, CI, pagination, default-deny auth, fire-and-forget audit, secrets removal, indexes). This handoff package supersedes the audit for current-state accuracy. The audit remains useful for the strategic roadmap and product-decision rationale.
- The repository memory file `/memories/repo/laohr-run-setup.md` documents the local run setup (PostgreSQL switch, license system, default users).

## Document index

| # | Document | Purpose |
|---|---|---|
| 00 | `00-README.md` | This file — how to use the package |
| 01 | `01-PROJECT-OVERVIEW.md` | Identity, objective, maturity |
| 02 | `02-REPOSITORY-MAP.md` | Directory tree + ownership |
| 03 | `03-TECH-STACK.md` | Full stack matrix with versions |
| 04 | `04-ARCHITECTURE.md` | Actual architecture + Mermaid diagrams |
| 05 | `05-RUNTIME-AND-DATA-FLOW.md` | Startup sequence + user-flow traces |
| 06 | `06-FRONTEND.md` | Next.js app, routes, components, design |
| 07 | `07-BACKEND.md` | .NET API, controllers, services, jobs |
| 08 | `08-DATABASE.md` | Schema, entities, ER diagram, risks |
| 09 | `09-API-INVENTORY.md` | Every endpoint |
| 10 | `10-FEATURE-INVENTORY.md` | Feature matrix by domain |
| 11 | `11-AUTH-AND-SECURITY.md` | JWT, refresh, RBAC, security review |
| 12 | `12-INFRASTRUCTURE-AND-DOCKER.md` | Docker, compose, healthchecks |
| 13 | `13-CICD.md` | GitHub Actions pipeline |
| 14 | `14-TESTING.md` | Test inventory + gaps |
| 15 | `15-OBSERVABILITY.md` | Logging, telemetry, health |
| 16 | `16-DESIGN-AND-UX.md` | Design system, theme, UX review |
| 17 | `17-CONFIGURATION.md` | Env vars, appsettings, config structure |
| 18 | `18-DEPENDENCIES.md` | Dependency analysis |
| 19 | `19-CODE-QUALITY.md` | Conventions, TODOs, dead code |
| 20 | `20-TECHNICAL-DEBT.md` | Debt register |
| 21 | `21-BUGS-AND-RISKS.md` | Bug/risk register |
| 22 | `22-MISSING-CAPABILITIES.md` | What's needed but absent |
| 23 | `23-IMPLEMENTATION-STATUS.md` | Explicit status report |
| 24 | `24-PRIORITY-BACKLOG.md` | Proposed backlog P0–P3 |
| 25 | `25-DEPENDENCY-GRAPH.md` | Prerequisite ordering of future work |
| 26 | `26-DEVELOPMENT-HISTORY.md` | Recent direction from git/structure |
| 27 | `27-DECISIONS-AND-CONSTRAINTS.md` | Technical decisions + constraints |
| 28 | `28-DO-NOT-BREAK.md` | Preservation rules |
| 29 | `29-LAST-KNOWN-GOOD-STATE.md` | Checkpoint for next AI |
| 30 | `30-CURRENT-DEVELOPMENT-FRONTIER.md` | Where work ends / unfinished begins |
| 31 | `31-AI-PROJECT-MEMORY.md` | High-density AI memory |
| 32 | `32-CONTINUATION-PACKAGE.md` | Paste-into-LLM continuation prompt |

## Evidence classification used throughout

- `VERIFIED` — confirmed by reading source/config
- `INFERRED` — derived but not directly confirmed
- `PARTIAL` — implemented but incomplete
- `BROKEN` — observable failure
- `PLANNED` — documented intent only, no code
- `UNUSED` / `DEPRECATED` — present but not active
- `UNKNOWN` — insufficient evidence

## Rules followed during extraction

- READ / ANALYZE / DOCUMENT ONLY — no code was modified.
- Every important claim is classified or cites a file path.
- Unknowns are explicitly marked; nothing is invented.