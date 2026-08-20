# 04 — UI / UX Review

> Reviewed: every page under `frontend/src/app`, every component under
> `frontend/src/components`, and the global stylesheet.
> Reference: `tailwindcss v4` is declared in `package.json` but CSS Modules are
> used everywhere (no `tailwind.config.js`). Light theme only.

## 1. Design System Reality

| Aspect                | Today                                                  |
| --------------------- | ------------------------------------------------------ |
| Theme                 | Light only. `<select>` on `/settings` for Dark/System is hard-coded `disabled`. |
| Tokens                | Defined ad-hoc in each CSS module. No `:root` CSS vars for color, spacing, radius, shadow. |
| Typography            | System font stack (no font import). No type-scale tokens. |
| Spacing               | Inline `padding: 1rem / 1.5rem / 2rem`. No 4/8 px scale. |
| Border radius         | Mostly `8px`, `12px`, `16px`. No `--radius-{sm,md,lg}` tokens. |
| Shadows               | Subtle, inlined. No `--shadow-{sm,md,lg}` tokens.      |
| Icons                 | Inline SVG per component. ~30 icons defined inside `Sidebar.tsx`, `page.tsx`. No `<Icon>` primitive. |
| Components            | `Button`, `Card`, `Input`, `Select`, `Modal`, `ConfirmationModal`, `Skeleton`, `MaskedField`, `LanguageSelector`. Small but inconsistent (see §5). |
| Charts                | **None.**                                               |
| Date picker           | None — uses native `<input type="date">`.               |
| Form validation       | Ad-hoc `errors` state per form.                        |
| Tables                | Inline per page. No shared `DataTable`.                 |
| Empty states          | Inconsistent. Some pages render blank.                  |
| Loading               | `Skeleton`, `SkeletonCard`, `SkeletonTable`. Inconsistent. |
| Error states          | Mostly `console.error` + `setError(text)`. One `alert()` in `ReportsPage`. |
| Toast                 | None.                                                   |

### Recommendation (P0/P1)

* Introduce `globals.css` with `:root` design tokens (`--color-*`, `--space-*`,
  `--radius-*`, `--shadow-*`, `--text-*`).
* Add Tailwind v4 `@theme { ... }` so tokens are reusable in utilities *and*
  CSS modules.
* Build `<Icon>` primitive that reads from a small set (~20 icons).
* Build `<DataTable>`, `<EmptyState>`, `<ErrorState>`, `<Toast>`,
  `<PageHeader>`, `<Breadcrumbs>` primitives.

## 2. Light Mode vs Dark Mode vs System

**Light mode works.** Dark mode does not exist. System preference is not
honored. The `/settings` page exposes a theme selector that is `disabled`.

Concrete evidence:

* `app/globals.css` contains no `@media (prefers-color-scheme: dark)` rules.
* No `.dark` or `[data-theme="dark"]` selectors in any module.
* `Sidebar.module.css`, `Card.module.css`, etc. use raw colors
  (`#ffffff`, `#f8f9fa`, `#e5e7eb`).

### Recommendation (P1)

* Define tokens once:

```css
:root {
  --color-bg: #f8f9fa;
  --color-surface: #ffffff;
  --color-fg: #0f172a;
  --color-muted: #64748b;
  --color-border: #e2e8f0;
  --color-primary: #6366f1;
  --color-success: #10b981;
  --color-warning: #f59e0b;
  --color-danger: #ef4444;
  --color-info: #0ea5e9;
}

@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    --color-bg: #0f172a;
    --color-surface: #1e293b;
    --color-fg: #f1f5f9;
    --color-muted: #94a3b8;
    --color-border: #334155;
  }
}

:root[data-theme="dark"] {
  /* overrides for explicit dark */
}
```

* Add a `ThemeProvider` that reads `localStorage`, falls back to
  `prefers-color-scheme`, and writes `data-theme` on `<html>`.
* Replace every raw color in CSS modules with `var(--color-…)`.
* Wire the `/settings` page selector to call the provider.

## 3. Layout Audit — Page by Page

### `/login` — `app/login/page.tsx`

* **Strengths**: clean hero, gradient orbs, full validation flow, accessible
  alert role on error.
* **Problems**:
  * Hard-coded gradient colors (`#6366f1`, `#4338ca`) — must become tokens.
  * No "forgot password" link.
  * No "show/hide password" toggle.
  * No language selector (forced to English; Lao users must change in
    `/settings` after login — but `/settings` is post-login).
* **Priority**: P2

### `/` (Dashboard) — `app/(dashboard)/page.tsx`

* **Strengths**: clean stat grid, quick action cards, skeleton placeholders.
* **Problems**:
  * "Recent Activity" is **mock data** (lines 128-143), not from the API.
    Misleading.
  * No role-aware widgets (HR sees leave stats, Employee sees my tasks).
  * No time-series (attendance trend over last 7 days).
  * No deep-links to filtered lists.
* **Priority**: P1 (replace mock data), P2 (role-aware widgets)

### `/employees` — `app/(dashboard)/employees/page.tsx`

* **Strengths**: search, filter by department/status, SkeletonTable on load.
* **Problems**:
  * Falls back to **mock data** on API failure (lines 49-58) — silently
    degrades. Should show an error toast instead.
  * No pagination — all employees in memory.
  * No column visibility / sort.
  * No bulk selection.
* **Priority**: P0 (no pagination + silent fallback)

### `/employees/new`, `/employees/[id]/edit`

* **Strengths**: reuses `EmployeeForm`.
* **Problems**:
  * `EmployeeForm` has fallback mock departments on API failure (same issue).
  * No address picker integration with `Province/District/Village`.
  * Form is hand-rolled; no central validation engine.
* **Priority**: P1

### `/employees/[id]` — detail

* **Strengths**: tabs (`personal`, `employment`, `documents`).
* **Problems**:
  * Documents tab is referenced but the page never fetches
    `/api/documents/employee/{id}` — feature is half-wired.
  * No "back to top" / no print.
  * No edit-in-place.
* **Priority**: P1

### `/attendance`

* **Strengths**: calendar + list view toggle, clock-in/out buttons.
* **Problems**:
  * Calendar component (`components/leave/LeaveCalendar.tsx`) is reused but
    may have attendance-specific UX issues.
  * Geolocation capture only works on HTTPS or `localhost` — should warn.
  * No filter by employee (HR use-case).
* **Priority**: P2

### `/leave`

* **Strengths**: tabs (`my-leave`, `approvals`, `calendar`), inline approve
  / reject with confirmation.
* **Problems**:
  * Falls back silently on API errors.
  * Hard-coded `formatRelativeTime` strings in English only.
  * Export uses native `alert()` failure.
  * No attachment preview.
* **Priority**: P2

### `/payroll`

* **Strengths**: period picker, slips table, Excel export.
* **Problems**:
  * ClosedXML export is fine for one period; will be slow for huge periods.
  * Slip detail not implemented (only list + PDF download).
  * No diff against previous period.
* **Priority**: P2

### `/reports`

* **Strengths**: simple NSSF download.
* **Problems**: only NSSF. No project reports, no custom reports, no
  scheduled emails.
* **Priority**: P2

### `/settings` and children

* **Strengths**: settings are clearly grouped.
* **Problems**:
  * `/settings` is mostly placeholder (theme/language disabled).
  * `/settings/company` does not validate address chain.
  * `/settings/work-schedule` does not surface Saturday preview.
  * `/settings/holidays` has no recurring-vs-fixed year view.
  * `/settings/leave` likely just lists policies; no quota projection.
  * `/settings/currency-rates` has no chart of rate history.
* **Priority**: P2

## 4. Navigation Audit

* The sidebar (`Sidebar.tsx`) is permission-filtered; only `payroll.view` and
  `settings.edit` are gated.
* The `/settings` group contains 5 sub-items rendered inline (not as a
  separate `/admin` page). For HR/Admin this is fine.
* There is **no breadcrumbs** except on `/employees/new`.
* There is **no top-bar search / command palette**.
* There is **no notification bell**.
* There is **no recent / favorites**.

### Recommendation

* Add `<Breadcrumbs>` via a `<PageHeader>` primitive.
* Add `<TopBar>` with `<Search>`, `<Notifications>`, `<ThemeToggle>`,
  `<UserMenu>`.
* Add mobile drawer (`<MobileNav>`).

## 5. Component Inconsistencies

| Component             | Variant / API                                | Issue                                            |
| --------------------- | -------------------------------------------- | ------------------------------------------------ |
| `Button`              | `variant: 'primary' | 'secondary' | 'ghost' | 'danger'` | Good.                                           |
| `Card`                | `variant: 'default' | 'elevated' | 'outlined'` | Good; but `default` and `elevated` look identical. |
| `Input`               | `size: 'sm' | 'md' | 'lg'`; left/right icon   | Good; but `Select` uses raw `<select>` element.  |
| `Modal`               | Generic dialog                               | No focus trap; no ESC handling; no return-focus.  |
| `ConfirmationModal`   | Specific                                      | Should reuse `Modal` + footer.                    |
| `Skeleton`            | `width` / `height` props                      | `SkeletonTable` exists but not used everywhere.   |
| `MaskedField`         | Single component                              | Used inconsistently across forms.                 |

## 6. Accessibility Audit

| Check                                  | Status      | Notes                              |
| -------------------------------------- | ----------- | ---------------------------------- |
| `Button` `aria-busy`                   | ✅          | `Button.tsx:52`                    |
| `Input` `aria-invalid` / `aria-describedby` | ✅      | `Input.tsx:60-65`                  |
| Modal focus trap                       | ❌          | Not implemented                    |
| Modal ESC close                        | ❌          | Not implemented                    |
| `<select>` accessible                  | ✅          | Native element                     |
| Color contrast                         | ⚠️          | Not formally verified              |
| Keyboard navigation                    | ⚠️          | Works for buttons, not tables      |
| `prefers-reduced-motion`               | ❌          | Animations always play             |
| Skip-to-content link                   | ❌          | Missing                            |
| Live region for toasts                 | ❌          | No toasts                          |
| ARIA roles on tables                   | ❌          | Inline `<table>` with no `<caption>` |

## 7. Internationalization Audit

* `lib/i18n.ts` defines `en` + `lo` dictionaries in one file.
* `LanguageProvider` (`components/providers/LanguageProvider.tsx`) wires it up.
* Many Lao strings are inline (Sidebar subItems, Settings page mostly).
* No `Intl.NumberFormat` for currency (raw `toLocaleString` in dashboard).
* No plural form support (`formatRelativeTime` uses English rules).
* Date formatting is centralized (`lib/datetime.ts`) — good.

### Recommendation (P1)

* Adopt `react-intl` or `i18next` with message catalogs.
* Move all hard-coded Lao strings into the dictionary.
* Use `Intl.NumberFormat('lo-LA', { style: 'currency', currency: 'LAK' })`
  in formatting helpers.
* Pluralize relative-time strings.

## 8. Forms Audit

* `EmployeeForm`, `LeaveRequestForm`, `NewPeriodModal`, `AdjustmentModal` are
  hand-rolled.
* Each implements its own `errors: Record<string, string>` state.
* No shared validators; no async validation (e.g., "check employee code
  uniqueness").
* No optimistic UI.
* No dirty-state warnings ("you have unsaved changes").

### Recommendation (P1)

* Standardize on `react-hook-form` + `Zod` (TS-first, tiny bundle).
* Provide `<FormField>` wrapper that connects label, helper, error.
* Centralize async validators.

## 9. Tables Audit

* Every page table is inline `<table>…</table>` with raw markup.
* No virtualization.
* No sort indicators (server-side sort absent).
* No column visibility.
* No sticky headers.
* No row selection.

### Recommendation (P0)

* Build a `<DataTable>` primitive.
* Use `@tanstack/react-virtual` for > 100 rows.
* Push filters + pagination into URL search params.

## 10. Empty / Loading / Error States

| Page             | Loading                          | Empty                              | Error                                   |
| ---------------- | -------------------------------- | ---------------------------------- | --------------------------------------- |
| Dashboard        | `Skeleton` in stat tiles         | Stats default to `0` (no empty)    | `console.error` only                    |
| Employees list   | `SkeletonTable`                  | "No employees found" message       | Silent fallback to **mock data** ❌     |
| Employee detail  | `SkeletonCard`                   | "Employee not found"               | `console.error`                         |
| Attendance       | `Skeleton`                       | Implicit "—"                       | `setError(text)`                        |
| Leave            | `SkeletonTable`                  | Implicit empty list                | `setError(text)`                        |
| Payroll          | `SkeletonTable`                  | Implicit empty                     | `setError(text)`                        |
| Reports          | Plain `setLoading(false)`        | N/A                                | `alert('Failed to download report')` ❌  |

### Recommendation (P1)

* Remove silent fallbacks to mock data. Surface real errors.
* Standardize `<EmptyState/>` with icon + title + CTA.
* Replace `alert()` with `<Toast/>`.

## 11. Mobile / Responsive

The CSS Modules do not consistently use `min-width` queries. The sidebar is a
fixed-width aside and will overflow on mobile. Tables will overflow. The
calendar will likely break.

### Recommendation (P1)

* Add `<MobileNav>` drawer for `<md`.
* Use `overflow-x: auto` wrappers around tables.
* Use Tailwind v4 responsive utilities consistently.
* Add a viewport meta tag check.

## 12. Animation

* Sidebar collapse uses CSS transform — OK.
* Modal fade-in exists but `prefers-reduced-motion` is not honored.
* Skeletons use `linear-gradient` background animation — OK but should honor
  `prefers-reduced-motion`.

## 13. Severity Summary

| Issue                                          | Severity |
| ---------------------------------------------- | -------- |
| Silent fallback to mock data on API errors     | **High** |
| No dark mode (selector exists, disabled)        | **High** |
| No pagination on lists                         | **High** |
| No shared `DataTable`                          | **High** |
| Hard-coded production credentials in repo      | **High** |
| `alert()` for errors                           | Medium   |
| No breadcrumbs / page titles / mobile nav      | Medium   |
| Inconsistent loading/empty states              | Medium   |
| Accessibility gaps (focus trap, reduced-motion) | Medium   |
| Hard-coded Lao strings                         | Low      |
| Mocked dashboard activity                      | Low      |
