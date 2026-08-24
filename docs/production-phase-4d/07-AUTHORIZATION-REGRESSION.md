# 07 — AUTHORIZATION REGRESSION

Re-run of representative security boundaries (existing suites, all passing):

- Employee ≠ HR, HR ≠ Finance, Procurement ≠ Accounting, Warehouse ≠ Finance
  (`FinanceAuthorizationTests`, `BackOfficeAuthorizationTests`).
- Corporate ≠ System Admin, Reception ≠ HR, Fleet Officer ≠ Payroll
  (`CorporateAuthorizationTests`).
- IDOR across Employee/Payroll/Recruitment/Performance/Project/Supplier/Purchase/
  Inventory/Asset/Finance/Documents/Contracts/Service Desk/Fleet/Travel/Visitors
  (existing `*IdorTests` + `CorporateAuthorizationTests`).

## Client-supplied authority audit

- Approver/actor identity is always resolved server-side (`ApprovalService`,
  `CurrentEmployeeService`, `SegregationOfDutiesService`).
- No DTO binds `ApprovedByUserId`/`Role`/`Permissions`/`CreatedBy`/`PostedBy`
  from the request body.
- Mass assignment: controllers use explicit request DTOs, not raw entity binding.

## Status

PASS (no new findings; existing suites green).
