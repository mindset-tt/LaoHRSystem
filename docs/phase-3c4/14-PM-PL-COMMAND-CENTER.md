# 14 — PM/PL Command Center

## Project detail workspace
Existing `/projects/[id]` page with tabs (board, list, activity, discussion). Phase 3C4 adds:
- Board (kanban) — via `/api/pm/projects/{id}/board`.
- Timeline (gantt) — via `/api/pm/projects/{id}/timeline`.
- Health — via `/api/pm/projects/{id}/health`.
- RAID (assumptions/decisions) — via `/api/pm/projects/{id}/assumptions|decisions`.

## Portfolio + Capacity
New pages `/portfolio` and `/capacity`.

## One task source of truth
List / Board / Gantt are views over the SAME tasks (no separate data).
