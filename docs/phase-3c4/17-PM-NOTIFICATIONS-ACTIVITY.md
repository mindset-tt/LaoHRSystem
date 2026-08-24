# 17 — PM Notifications / Activity

## Activity
Existing `ActivityLog` powers the project feed (task changes, milestone, risk, issue, resource changes). Kanban move logs `UPDATED_STATUS`.

## Notifications
Reuse Phase 3C2 `NotificationService`. High-value events only (TaskAssigned, TaskDueSoon, TaskOverdue, RiskAssigned, IssueAssigned) — NOT every drag/reorder.

## Activity vs notification
Activity = historical project feed. Notification = action/relevance to a specific user. Not conflated.

## Notification fatigue
No notifications for cosmetic board reordering.
