# 08 — Purchase Request

## Entities
- `PurchaseRequest`: RequestNumber, RequestedByEmployeeId, DepartmentId, ProjectId,
  CostCenterId, RequiredDate, Purpose, Status, TotalEstimatedAmount, Currency.
- `PurchaseRequestItem`: Description, ItemId (optional catalog), Quantity, Unit,
  EstimatedUnitPrice, EstimatedAmount, PreferredSupplierId, Notes.

## Flow
1. Employee creates PR (DRAFT) — self-service.
2. Employee submits → `PENDING_APPROVAL` + approval request created.
3. Approver (direct manager → Admin/Finance) approves → `APPROVED`.
4. Procurement converts to PO → `CONVERTED`.

## Status
`DRAFT`, `PENDING_APPROVAL`, `APPROVED`, `REJECTED`, `CANCELLED`, `CONVERTED`.

## Invariants
- Cannot create PO from a non-APPROVED request.
- Approver identity resolved server-side (never client-supplied).
- Employees see only their own requests (IDOR-guarded).

## Numbering
`PR-{yyyy}-{000000}` via `NumberSequenceService`.
