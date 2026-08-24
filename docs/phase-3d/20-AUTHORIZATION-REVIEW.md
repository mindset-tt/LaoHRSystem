# 20 — Authorization Review

## Model
- Role-based access control (RBAC): Admin, HRAdmin, Manager, Employee.
- Policy-based authorization on controllers.
- Org-hierarchy scoping (manager sees direct reports).

## Findings
- Authorization attributes present on controllers.
- Org-hierarchy scoping implemented (Phase 3B).

## Follow-up
- Verify no missing `[Authorize]` on sensitive endpoints (audit).
- Verify IDOR protection (resource ownership checks) on all entity endpoints.
- Verify manager scope does not leak sibling/peer data.
- Least-privilege review of roles.
