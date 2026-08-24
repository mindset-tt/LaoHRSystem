# 09 — Authorization / IDOR Matrix

| Resource | Create (IDOR) | Read (IDOR) | Approve/Reject (spoofing) |
|---|---|---|---|
| Leave | Server-resolved EmployeeId | List accepts `employeeId` filter (read path not self-scoped — noted) | Server-resolved actor via ApprovalService |
| Expense | Server-resolved EmployeeId | List accepts `employeeId` filter | Server-resolved actor |
| Loan | Server-resolved EmployeeId | List accepts `employeeId` filter | Server-resolved actor |
| Attendance correction | Server-resolved EmployeeId | `GetCorrection` not self-scoped (noted) | Server-resolved actor |
| Approval detail | — | Restricted to requester/approver/HR-Admin (403 otherwise) | Server-resolved actor |
| Notification | — | Scoped to current UserId | Scoped to current UserId |

## Tests
- `ApprovalServiceTests` (approver spoofing, terminal-state, snapshot).
- `NotificationIdorTests` (cross-user read/mark-read).

## Known remaining read-path gaps (documented, not blocking Phase 3C2B)
- `GET /api/leave?employeeId=` and `GET /api/expenses?employeeId=` and `GET /api/employeeLoans?employeeId=` accept a client-supplied `employeeId` filter. These are list endpoints intended for HR/Admin; a non-privileged user could enumerate other employees' records. Full read-path scoping is deferred to a dedicated authorization pass (Phase 3C3+).
