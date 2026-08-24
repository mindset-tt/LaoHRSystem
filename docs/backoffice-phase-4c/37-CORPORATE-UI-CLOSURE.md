# 37 — CORPORATE UI CLOSURE

Phase 4C.1 frontend closure: every major corporate domain is now navigable and
usable from the web application.

## Navigation

Grouped "Corporate" sidebar entry (permission `corporate.view`) with sub-items:
Dashboard, Documents, Contracts, Service Desk, Facilities, Rooms, Maintenance,
Fleet, Travel, Visitors.

## Pages implemented

| Domain | Route | List | Create | Action |
|---|---|---|---|---|
| Dashboard | `/corporate` | ✓ (KPI cards) | — | — |
| Documents | `/corporate/documents` | ✓ | ✓ (metadata) | — |
| Contracts | `/corporate/contracts` | ✓ | ✓ | — |
| Service Desk | `/corporate/service-desk` | ✓ | ✓ | — |
| Facilities | `/corporate/facilities` | ✓ | ✓ | — |
| Rooms | `/corporate/facilities/rooms` | ✓ | ✓ | — |
| Maintenance | `/corporate/maintenance` | ✓ | ✓ | — |
| Fleet | `/corporate/fleet` | ✓ | ✓ | — |
| Travel | `/corporate/travel` | ✓ | ✓ | — |
| Visitors | `/corporate/visitors` | ✓ | ✓ | check-in/out |

## UX

- Loading/empty/error states via `DataTable` + `EmptyState` + toast.
- Server remains authoritative (conflict/forbidden surfaced via toast).
- Dark/light via existing design tokens; i18n keys added (English + Lao).

## Status

PASS.
