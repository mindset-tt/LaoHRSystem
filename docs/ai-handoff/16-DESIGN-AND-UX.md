# 16 — Design & UX Review

> `VERIFIED` from `frontend/src/app/globals.css` + component structure (subagent).

## Visual language

Hand-authored CSS variables in `globals.css` (no `tailwind.config.js`; Tailwind v4 PostCSS registered but design system is CSS-vars-driven). Components use CSS Modules (`*.module.css`).

## Typography

- **UI font**: Inter (Google Fonts).
- **Lao text font**: Noto Sans Lao (Google Fonts).
- Loaded via `@import` in `globals.css`.

## Design tokens (CSS custom properties)

- **Primary**: Deep Indigo scale (`--color-primary-50…900`, 500=#6366f1).
- **Accent**: Warm Amber (`--color-accent-*`, 500=#f59e0b).
- **Semantic**: success (green), warning (amber), error (red).
- **Neutral**: warm grays 50–900.
- **Surface/background/border/text** tokens.
- Tokens stored as RGB channel triples for alpha compositing.

## Theme / dark mode

- `ThemeProvider` supports `light` / `dark` / `system` via `data-theme` attribute on `<html>`.
- `:root[data-theme="dark"]` overrides surface/background/border/text tokens.
- `:root:not([data-theme="light"])` applies dark as fallback.
- Persisted to `localStorage['laohr:theme']`.
- **Status**: DONE (audit previously said "fake" — now implemented).

## Spacing / radius / shadow

Defined as CSS variables in `globals.css` (`INFERRED` — consistent token set).

## Navigation

- `Sidebar`: collapsible, permission-filtered items, sub-menus (Finance, Knowledge, Settings).
- `Header`: greeting/breadcrumbs/title + actions + `LanguageSelector`.
- `Breadcrumbs`: `Breadcrumbs` component (inconsistent usage — only some pages).

## Information hierarchy

`PageHeader` (title/subtitle/breadcrumbs) primitive exists but inconsistent adoption. Some pages use inline headers.

## Component consistency

- Primitives: Button, Input, Select, Card, Modal, ConfirmationModal, DataTable, Pagination, Form, PageHeader, Breadcrumbs, Toast, Skeleton, EmptyState/ErrorState, MaskedField, LanguageSelector, CommentThread.
- Inconsistencies: `EmployeeForm` uses manual `useState` while other forms use react-hook-form + zod.

## Responsive / mobile

Desktop-first. Sidebar collapsible. **No off-canvas mobile drawer**. `INFERRED` mobile/tablet layouts untested/likely broken.

## Accessibility

- Minimal: `aria-busy` on some buttons.
- **No focus trap** in Modal confirmed.
- `prefers-reduced-motion` not honored.
- Keyboard navigation limited.
- `INFERRED` low accessibility compliance.

## Dashboard density

Spacious. No `compact` density toggle.

## Missing product interfaces (domain-relevant)

| Interface | Required? | Status |
|---|---|---|
| Dashboard analytics/charts | USEFUL | NOT_STARTED (no chart lib; dashboard is 4 scalars + mock activity) |
| Gantt | USEFUL (PM) | NOT_STARTED |
| Timeline | OPTIONAL | NOT_STARTED |
| Activity feed | USEFUL | PLACEHOLDER (mock data in dashboard) |
| Notification center | REQUIRED | NOT_STARTED |
| Command palette / global search | USEFUL | NOT_STARTED |
| Audit log UI | USEFUL | NOT_STARTED (API only) |
| User management admin console | REQUIRED | NOT_STARTED |
| Reporting (visual) | USEFUL | PARTIAL (downloads only, no charts) |
| Saved views / filters in URL | USEFUL | NOT_STARTED |
| Mobile nav drawer | REQUIRED | NOT_STARTED |
| `/403` page | REQUIRED | MISSING (referenced, not created) |

## UX gaps

- Mock data in dashboard activity feed (misleading).
- Mock data fallback in `employees/[id]/edit` (masks API failures).
- Mock document data in `employees/[id]` detail.
- No toast/error consistency (legacy `alert()` may remain).
- Loading skeletons inconsistent across pages.
- No error boundaries (render crash kills page).