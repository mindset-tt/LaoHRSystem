# 06 — Kanban

## Endpoint
`GET /api/pm/projects/{id}/board` returns columns (TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE) with ordered cards.

## Move
`POST /api/pm/projects/{id}/board/move` — server validates authorization, status, task existence, and updates status + optional SortOrder. Sets/clears `CompletedAt` on DONE transitions. Logs `UPDATED_STATUS` activity.

## Drag/drop
Frontend drag is NOT the source of truth; backend validates. Keyboard alternative (status change) required.

## Concurrency
No optimistic concurrency token yet (deferred); stale moves are last-write-wins but never corrupt data (single-field status update).

## Ordering
`SortOrder` integer; no full resequencing on every move.
