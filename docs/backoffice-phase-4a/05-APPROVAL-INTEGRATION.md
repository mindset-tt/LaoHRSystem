# 05 — Approval Integration

## Reuse
The existing `ApprovalService` (polymorphic, sequential steps, server-side
approver resolution) is reused for Back Office. No new workflow engine.

## New approval type
`PURCHASE_REQUEST` added. Routing:
1. `DIRECT_MANAGER` (or `ROLE=HR` if no manager).
2. `ROLE=Admin` (Finance) for final approval.

## Approver resolution (server-side only)
- `EMPLOYEE`, `DIRECT_MANAGER`, `DEPARTMENT_MANAGER`, `ROLE`.
- `ApprovedById` is never trusted from the client.

## Extensible approval types (future)
`PURCHASE_ORDER`, `BUDGET`, `EXPENSE`, `SUPPLIER_INVOICE`, `PAYMENT`,
`STOCK_ADJUSTMENT`, `ASSET_DISPOSAL`, `CONTRACT` — only where business workflow
requires. Not all implemented in Phase 4A (only PURCHASE_REQUEST).

## Notifications
`NotificationService` reused. New notification types emitted:
- `APPROVAL_REQUESTED`, `APPROVAL_APPROVED`, `APPROVAL_REJECTED` for purchase requests.
