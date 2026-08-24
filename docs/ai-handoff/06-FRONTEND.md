# 06 — Frontend Analysis

> `VERIFIED` from subagent exploration of `frontend/`.

## Framework

Next.js 16.1.1 (App Router, Turbopack, React Compiler enabled) + React 19.2.3 + TypeScript 5 (strict). All pages are `'use client'` — client-rendered SPA-style; no server components doing data fetching.

## Routing

App Router with a `(dashboard)` route group sharing a Sidebar+Header shell. No route handlers (`route.ts`) — all API calls go client-side. Middleware is `proxy.ts` (not the conventional `middleware.ts`) with redirect logic **commented out** — currently a no-op pass-through.

## Layout system

- Root `layout.tsx` wraps providers: `ThemeProvider → LanguageProvider → ToastProvider → AuthProvider`.
- `(dashboard)/layout.tsx` calls `useRequireAuth()` and renders `Sidebar` + `Header` + content; shows skeleton while auth loads.

## Page inventory

| Route | File | Purpose | Status |
|---|---|---|---|
| `/` (layout) | `app/layout.tsx` | Root layout + providers | COMPLETE |
| `/login` | `app/login/page.tsx` | Username/password login | COMPLETE |
| `/` | `(dashboard)/page.tsx` | Dashboard: stats widgets + quick actions (activity feed is MOCK data) | PARTIAL (mock activity) |
| `/my-tasks` | `my-tasks/page.tsx` | Personal assigned tasks (paginated) | COMPLETE |
| `/employees` | `employees/page.tsx` | List w/ search, filters, server pagination (DataTable) | COMPLETE |
| `/employees/new` | `employees/new/page.tsx` | Create employee (HR/Admin gated) | COMPLETE |
| `/employees/[id]` | `employees/[id]/page.tsx` | Detail w/ tabs (Personal/Employment/Documents — documents mocked) | PARTIAL (mock docs) |
| `/employees/[id]/edit` | `employees/[id]/edit/page.tsx` | Edit (mock data fallback on error) | PARTIAL (mock fallback) |
| `/attendance` | `attendance/page.tsx` | Calendar/list toggle, clock in/out, live clock | COMPLETE |
| `/leave` | `leave/page.tsx` | Tabs: my-leave/approvals/calendar, balances, approve/reject, CSV export | COMPLETE |
| `/payroll` | `payroll/page.tsx` | Periods, run, slips, adjustments modals, masked salary | COMPLETE |
| `/projects` | `projects/page.tsx` | Project list (status/priority/search/mineOnly) | COMPLETE |
| `/projects/[id]` | `projects/[id]/page.tsx` | Project detail: board/list/activity/discussion tabs, Kanban | COMPLETE |
| `/projects/[id]/resources` | `projects/[id]/resources/page.tsx` | Resource allocation | COMPLETE |
| `/projects/[id]/issues` | `projects/[id]/issues/page.tsx` | Issues list | COMPLETE |
| `/projects/[id]/issues/[issueId]` | `.../issues/[issueId]/page.tsx` | Issue detail w/ comments | COMPLETE |
| `/projects/[id]/risks` | `projects/[id]/risks/page.tsx` | Risks list | COMPLETE |
| `/projects/[id]/risks/new` | `.../risks/new/page.tsx` | New risk form (Zod) | COMPLETE |
| `/projects/[id]/tasks/[taskId]` | `.../tasks/[taskId]/page.tsx` | Task detail w/ progress slider + comments | COMPLETE |
| `/finance/expenses` | `finance/expenses/page.tsx` | Expenses list | COMPLETE |
| `/finance/expenses/new` | `finance/expenses/new/page.tsx` | New expense (Zod) | COMPLETE |
| `/finance/expenses/[id]` | `finance/expenses/[id]/page.tsx` | Expense detail (approve/reject) | COMPLETE |
| `/finance/loans` | `finance/loans/page.tsx` | Loans list | COMPLETE |
| `/finance/loans/new` | `finance/loans/new/page.tsx` | New loan (Zod) | COMPLETE |
| `/finance/loans/[id]` | `finance/loans/[id]/page.tsx` | Loan detail (record repayment) | COMPLETE |
| `/knowledge` | `knowledge/page.tsx` | Articles list | COMPLETE |
| `/knowledge/new` | `knowledge/new/page.tsx` | New article (Zod) | COMPLETE |
| `/knowledge/[id]` | `knowledge/[id]/page.tsx` | Article detail | COMPLETE |
| `/knowledge/announcements` | `knowledge/announcements/page.tsx` | Announcements list | COMPLETE |
| `/knowledge/announcements/new` | `.../announcements/new/page.tsx` | New announcement (Zod) | COMPLETE |
| `/knowledge/announcements/[id]` | `.../announcements/[id]/page.tsx` | Announcement detail (auto-mark read) | COMPLETE |
| `/reports` | `reports/page.tsx` | NSSF package download (zip) — no charts | PARTIAL (no charts) |
| `/settings` | `settings/page.tsx` | Settings hub + profile + language/theme | COMPLETE |
| `/settings/company` | `settings/company/page.tsx` | Company info + cascading address | COMPLETE |
| `/settings/work-schedule` | `settings/work-schedule/page.tsx` | Work schedule config | COMPLETE |
| `/settings/holidays` | `settings/holidays/page.tsx` | Holiday CRUD | COMPLETE |
| `/settings/leave` | `settings/leave/page.tsx` | Leave policy quotas | COMPLETE |
| `/settings/currency-rates` | `settings/currency-rates/page.tsx` | Conversion rates w/ history | COMPLETE |

**Missing pages**: `/403` (referenced by `useRequirePermission` redirect but does not exist — `BROKEN`); `/finance` root (no `page.tsx`, orphaned `page.module.css`).

## Components

All custom — **no UI library** (no shadcn/MUI/Radix). Styling via CSS Modules (`*.module.css`).

- **`ui/`**: Button, Input, Select, Card, Modal, ConfirmationModal, DataTable (generic, client-side sort, empty states), Pagination, Form (react-hook-form + zod wrapper), PageHeader, Breadcrumbs, Toast (+`useToast`), Skeleton, EmptyState/ErrorState, MaskedField, LanguageSelector, CommentThread.
- **`layout/`**: Sidebar (collapsible, permission-filtered, sub-menus), Header (greeting + LanguageSelector).
- **`forms/`**: EmployeeForm (manual useState — inconsistent with react-hook-form elsewhere), LeaveRequestForm, NewPeriodModal, AdjustmentModal.
- **`leave/`**: LeaveCalendar (monthly calendar, color-coded).
- **`providers/`**: AuthProvider (`useAuth`, `useRequireAuth`, `useRequirePermission`), ThemeProvider (light/dark/system, `localStorage['laohr:theme']`), LanguageProvider (`useLanguage`, `localStorage['language']`).

## UI library / design system

Hand-authored CSS variables in `globals.css` (no `tailwind.config.js`). Tailwind v4 PostCSS plugin registered but design system is CSS-vars-driven. See `16-DESIGN-AND-UX.md` for token details.

## Theme / dark mode

ThemeProvider supports `light`/`dark`/`system` via `data-theme` attribute on `<html>`. Dark mode tokens defined in `:root[data-theme="dark"]`. `VERIFIED` (audit previously said "fake" — this is now implemented).

## Responsive / mobile

Desktop-first. Sidebar is collapsible. No off-canvas mobile drawer implemented. `INFERRED` mobile is untested/likely broken.

## State management

React Context only (Auth, Theme, Language, Toast). No Redux/Zustand. No server-state cache (no SWR/React Query) — data refetched on mount/navigation. `apiClient` has a `refreshPromise` singleton to dedupe concurrent token refreshes.

## API integration

`lib/apiClient.ts`: native `fetch`, base URL `NEXT_PUBLIC_API_URL || 'http://localhost:5000'`, `credentials: 'include'`, token in `localStorage`, auto-refresh on expiry (<1min) or 401 (single retry). `ApiClientError` class. Typed endpoint modules in `lib/endpoints/` (auth, employees, attendance, leave, payroll, reports, schedule, conversionRates, adjustments, projects, finance, knowledge, company).

⚠️ **Base URL mismatch**: `apiClient.ts` defaults to `http://localhost:5000`; Dockerfile bakes `http://localhost:8080`. In Docker, `NEXT_PUBLIC_API_URL` env must be set correctly.

## Auth UI

Login → POST `/api/auth/login` → store tokens in `localStorage` → AuthContext. Protected routes via `useRequireAuth()` (redirect to `/login`). Permission gating via `useRequirePermission()` (redirect to `/403` — page missing). `proxy.ts` middleware redirect is commented out.

## Forms / validation

`react-hook-form` + `zod` via custom `<Form>` primitive (`FormFormField`/`FormSelectField`/`FormGrid`). Used by risk/loan/expense/article/announcement forms. `EmployeeForm` uses manual `useState` (inconsistent).

## Tables / charts

- **Tables**: custom `DataTable<T>` generic (column defs, client-side sort, empty states, row click) + `Pagination` + `PaginatedResponse<T>` envelope (server-side pagination, default 25).
- **Charts**: **None.** No charting library. Reports page only downloads NSSF zip.

## Localization / i18n

Custom dictionary in `lib/i18n.ts` (`en`/`lo`). `LanguageProvider` exposes `t`. Persisted to `localStorage['language']`. `scripts/check-i18n.mjs` checks parity (run via `npm run i18n:check`). `i18n-audit.ts` runtime dev check. Some inline Lao strings in `settings/page.tsx` (inconsistent).

## Notifications / error handling / loading / empty states

- Toast: `<Toast>` + `useToast()` (info/success/warning/error).
- Loading: `Skeleton`/`SkeletonCard`/`SkeletonTable` (inconsistent usage).
- Empty: `EmptyState`/`ErrorState` primitives.
- Some legacy `alert()` calls may remain.

## Accessibility

Minimal. `aria-busy` on some buttons. No focus trap in Modal confirmed, `prefers-reduced-motion` not honored. `INFERRED` low.

## Performance

No list virtualization (10k rows will render all). No `@next/bundle-analyzer` wired. Bundle is lean (no UI/chart/state libs) — currently OK. See `06-scalability-performance.md` in audit for budgets.