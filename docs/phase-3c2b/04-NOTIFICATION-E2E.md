# 04 — Notification End-to-End

## Emitted notifications (verified in code)
- Leave: submitted (approver notified), approved (requester notified), rejected (requester notified).
- Expense: submitted (approver notified), approved/rejected (requester notified).
- Loan: approved/rejected (requester notified). Loan approval request is lazily created on first approval action (DRAFT lifecycle).
- Attendance correction: submitted (approver notified), approved/rejected (requester notified).

## Recipient correctness
- Submission → current approver (resolved `ManagerId` or HR role fallback).
- Approval/rejection → requester.
- No linked AppUser → `NotifyEmployeeAsync` no-ops gracefully; business action still succeeds.

## Security (IDOR)
`NotificationsController` filters all reads/writes by the current `UserId` (from JWT `UserId` claim, fallback username lookup). A user cannot read or mark another user's notification.

## Tests
`NotificationIdorTests`:
- `MarkRead_OtherUsersNotification_ReturnsNotFound`
- `List_OnlyReturnsOwnNotifications`

## Pagination
`GET /api/notifications` is paged (`page`, `pageSize`, capped at 100), ordered newest-first, with `unreadOnly` filter and `unread-count` endpoint.

## Indexes
`(UserId, IsRead, CreatedAt)` composite index supports the list/unread-count hot path.
