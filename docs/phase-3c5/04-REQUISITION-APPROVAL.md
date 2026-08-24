# 04 — Requisition Approval

## Reuse
`ApprovalService` (Phase 3C1). Requisition submit creates an `ApprovalRequest` with a single `ROLE=HR` step.

## Server-side approver resolution
Approver identity is resolved server-side (never client-supplied `ApprovedByEmployeeId`).

## Flow
`submit` → status `PENDING_APPROVAL` + approval request → `approve` (HR) → status `APPROVED`.

## Minimal policy
Single HR approval step (configurable later). No multi-level bureaucracy invented.
