# 04 — Authorization Model

## Current roles
`AppUser.Role` (string): `Admin`, `HR`, `Employee`. No separate permission table.

## Phase 4A extension
Added `IBackOfficeAccessService` (`BackOfficeAccessService`) to centralize
module-level access checks without scattering role checks across controllers.

| Capability | Admin | HR | Employee |
|---|---|---|---|
| View/Manage Procurement | ✅ | ✅ | ❌ (self-service create only) |
| Approve Procurement | ✅ | ✅ | ❌ |
| View/Manage Finance (budgets, AP) | ✅ | ❌ | ❌ |
| View/Manage Inventory | ✅ | ✅ | ❌ |
| Adjust Inventory | ✅ | ✅ | ❌ |
| View/Manage Assets | ✅ | ✅ | ❌ |
| Assign Assets | ✅ | ✅ | ❌ |
| View/Manage Contracts | ✅ | ✅ | ❌ |
| Manage Service Requests | ✅ | ✅ | ❌ (self-service create only) |

## Contextual authorization
- Department manager may approve their team's request (via `ApprovalService`
  `DIRECT_MANAGER` resolver) — does NOT grant Finance access.
- Procurement user manages POs — does NOT grant payroll access.

## Segregation of duties (foundation)
- Requester ≠ final approver (approver resolved server-side, never client-supplied).
- `ApprovedById` is NEVER accepted from the frontend.

## Frontend (UX-only)
`permissions.ts` role→permission matrix extended with `procurement.view`,
`inventory.view`, `assets.view`, `contracts.view`, `finance.view`. Backend is
authoritative; hiding a menu item is UX only.
