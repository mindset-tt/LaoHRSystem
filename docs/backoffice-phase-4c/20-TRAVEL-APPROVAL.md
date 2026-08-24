# 20 — TRAVEL APPROVAL

## Design

Reuses `ApprovalService` with request type `TRAVEL_REQUEST`. Approver resolved
server-side (direct manager); client never supplies approver identity.

## Flow

`submit` → PENDING_APPROVAL; `approve` → APPROVED (notifies employee);
`reject` → CANCELLED (notifies employee).

## Status

PASS.
