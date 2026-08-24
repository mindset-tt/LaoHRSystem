# 21 — Audit Production

## Audit log
- `AuditLog` entity + `AuditLogsController`.
- Audit middleware/service records sensitive actions.
- Retention: `Retention__AuditLogDays=365` (default).

## Findings
- Audit logging implemented (Phase 3C).
- Retention configurable but default disabled (`Retention__Enabled=false`).

## Follow-up
- Enable retention in production.
- Verify audit covers: auth events, payroll changes, permission changes, data exports.
- Ensure audit log is append-only and tamper-evident.
- Export/archive audit logs before retention purge.
