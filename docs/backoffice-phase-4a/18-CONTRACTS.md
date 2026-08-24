# 18 — Contracts

## Entity
`Contract`: ContractNumber, Title, SupplierId, ContractType, OwnerEmployeeId,
DepartmentId, StartDate, EndDate, Amount, Currency, Status, RenewalType, NoticeDate.

## Status
`DRAFT`, `ACTIVE`, `EXPIRED`, `TERMINATED`.

## Invariants
- Expired/terminated contract history is retained (no physical delete).
- Renewal creates history/version rather than destroying the old record.

## Documents
Actual contract file uses existing document infrastructure (deferred).

## Reminders
Notifications for expiry/renewal (deferred — no new notification system).

## Numbering
`CTR-{yyyy}-{000000}` via `NumberSequenceService`.

## Access
Contract-gated (Admin/HR).
