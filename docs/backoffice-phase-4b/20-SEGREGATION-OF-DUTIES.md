# 20 — Segregation of Duties

## Foundation
Finance requires stronger control. Configurable controls (not hardcoded):
- Invoice creator ≠ final approver.
- Payment creator ≠ payment approver.
- Payment approver ≠ recipient/vendor administrator.

## Current state
Permissions are designed to support segregation (separate create/approve
capabilities), but the actual "creator ≠ approver" enforcement is a configurable
follow-up (no hardcoded policy).

## Tests
`FinanceAuthorizationTests` proves HR/Employee cannot access finance surfaces.
Creator-vs-approver enforcement is documented as a follow-up (not yet wired).
