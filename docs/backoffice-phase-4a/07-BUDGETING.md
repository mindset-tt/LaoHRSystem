# 07 — Budgeting

## Entity
`Budget`: FiscalYear, DepartmentId, ProjectId, CostCenterId, Category, Currency,
ApprovedAmount, Status.

## Formula (no double counting)
```
Available = ApprovedAmount - Committed - Actual
```
- **Committed**: open POs (SENT / PARTIALLY_RECEIVED / RECEIVED) linked via CostCenter.
- **Actual**: PAID expenses (AmountLak).

## Cost Center
`CostCenter` (Code, Name, NameLao, DepartmentId, Status) — financial tracking
structure, distinct from `Department` (org structure). Not duplicated.

## Scope
Lightweight annual budgeting only. No monthly/quarterly allocation, no FP&A
engine, no predictive AI. Budget consumption is not yet enforced as a hard
block on PR/PO creation (documented as a follow-up).

## Access
Finance-gated (`CanViewFinance`/`CanManageFinance` = Admin only).
