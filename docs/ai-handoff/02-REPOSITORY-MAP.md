# 02 — Repository Map

> `VERIFIED` via `list_dir` + subagent exploration.

## Top-level tree

```
d:\LaoHRSystem/                       repository root (git branch: master)
├── README.md                         project README (partially stale re: SQL Server)
├── docker-compose.yml                Phase 6e — postgres + api + web stack
├── .env.example                      env var template (secrets placeholders)
├── .github/workflows/ci.yml          Phase 6f — CI pipeline
├── docs/
│   ├── audit/                        prior strategic audit (00–13) — superseded for current-state
│   └── ai-handoff/                   THIS package
├── Backend/                          .NET 10 solution (no .sln file)
│   ├── LaoHR.API/                    ASP.NET Core 10 web API (main app)
│   ├── LaoHR.Shared/                 shared entities + DbContext + services
│   ├── LaoHR.Bridge.Service/         Windows Service (ZKTeco attendance sync)
│   ├── LaoHR.Bridge.Config/          WPF desktop app (ZKTeco device config)
│   ├── LaoHR.LicenseGen/             offline license-key generator CLI
│   └── LaoHR.Tests/                  xUnit test project
└── frontend/                         Next.js 16 + React 19 app
    ├── package.json
    ├── Dockerfile
    ├── next.config.ts
    ├── tsconfig.json
    ├── eslint.config.mjs
    ├── postcss.config.mjs
    ├── scripts/check-i18n.mjs
    ├── public/                       default Next.js SVGs only
    └── src/
        ├── proxy.ts                  Next.js middleware (redirect logic commented out)
        ├── app/                      App Router pages
        ├── components/               UI + layout + forms + providers
        └── lib/                      apiClient, types, i18n, permissions, endpoints/
```

## Major directories explained

| Directory | Purpose | Ownership | Key dependencies | Entry point | Actively used |
|---|---|---|---|---|---|
| `Backend/LaoHR.API` | Main REST API | Backend team | ASP.NET Core, EF Core (Npgsql + InMemory), Serilog, OpenTelemetry, QuestPDF, ClosedXML, iText7, FluentValidation | `Program.cs` | ✅ Yes |
| `Backend/LaoHR.Shared` | Shared entities, DbContext, license service | Backend | EF Core SqlServer (referenced but runtime uses Npgsql), Newtonsoft.Json | `Data/LaoHRDbContext.cs`, `Entities.cs` | ✅ Yes |
| `Backend/LaoHR.Bridge.Service` | Windows Service for scheduled ZKTeco attendance sync | Backend | EF Core SqlServer, `Microsoft.Extensions.Hosting.WindowsServices` | `Program.cs`, `Worker.cs` | ⚠️ Partial — still references SQL Server, not migrated to Npgsql |
| `Backend/LaoHR.Bridge.Config` | WPF desktop app for ZKTeco device config | Backend | EF Core SqlServer, WPF | `MainWindow.xaml.cs`, `ZkDevice.cs` | ⚠️ Partial — still references SQL Server |
| `Backend/LaoHR.LicenseGen` | Offline license-key generator CLI | Backend | Newtonsoft.Json | `Program.cs` | ✅ Yes (utility) |
| `Backend/LaoHR.Tests` | xUnit test suite | QA | xUnit, Moq, FluentAssertions, Bogus, Mvc.Testing | `LaoHR.Tests.csproj` | ✅ Yes |
| `frontend` | Next.js web app | Frontend | Next 16, React 19, Tailwind v4, react-hook-form, zod | `src/app/layout.tsx` | ✅ Yes |
| `docs/audit` | Prior strategic audit + roadmap | — | — | — | Reference only (superseded) |
| `docs/ai-handoff` | This handoff package | — | — | — | This document |

## Notable absences / hidden folders

- **No `.sln` file** — projects are built individually. `VERIFIED`.
- **No `.vscode/`, `.devcontainer/`, `Makefile`, `Taskfile.yml`** — `VERIFIED` by listing.
- **No `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`, `COPILOT.md`, `.cursor/`, `.claude/`, `.agents/`** AI-instruction files in the repo — `VERIFIED`. (There is an external skill at `c:\Users\rl34996\.agents\skills\` but it is not part of the repo.)
- **No `tailwind.config.js`** — Tailwind v4 uses CSS-based config; design tokens are hand-authored CSS variables in `globals.css`. `VERIFIED`.
- **No `middleware.ts`** — the middleware is named `proxy.ts`; Next.js convention expects `middleware.ts` exporting `middleware` — see `06-FRONTEND.md` for the wiring concern. `VERIFIED`.

## Frontend route groups

```
src/app/
├── layout.tsx                         root layout (Theme→Language→Toast→Auth providers)
├── login/page.tsx                     login
└── (dashboard)/                       route group — shared Sidebar+Header shell
    ├── layout.tsx                     useRequireAuth gate
    ├── page.tsx                       dashboard home
    ├── my-tasks/page.tsx              personal task list
    ├── employees/                     list, new, [id], [id]/edit
    ├── attendance/page.tsx
    ├── leave/page.tsx
    ├── payroll/page.tsx
    ├── projects/                      list, [id], [id]/resources, [id]/issues, [id]/risks, [id]/tasks/[taskId]
    ├── finance/                       expenses (list/new/[id]), loans (list/new/[id])
    ├── knowledge/                     articles (list/new/[id]), announcements (list/new/[id])
    ├── reports/page.tsx
    └── settings/                      hub + company + work-schedule + holidays + leave + currency-rates
```

`VERIFIED` from subagent route inventory. Note: `finance/page.tsx` is missing (orphaned `finance/page.module.css` exists); `/403` page does not exist despite `useRequirePermission` redirecting there.