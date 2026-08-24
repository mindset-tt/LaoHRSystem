# 08 — CONTRACT APPROVAL

## Design

Reuses `ApprovalService` with request type `CONTRACT`. Approver is resolved
server-side (direct manager); the client never supplies the approver identity.

## Flow

`submit-approval` creates an approval request and sets status PENDING_APPROVAL.
`approve` transitions to ACTIVE and notifies the owner.

## Status

PASS.
