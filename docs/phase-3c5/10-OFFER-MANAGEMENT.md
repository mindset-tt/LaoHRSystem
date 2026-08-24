# 10 — Offer Management

## Model
`Offer` (ApplicationId, PositionId, ProposedStartDate, Salary, Currency, EmploymentType, Status, CreatedByEmployeeId, ExpiresAt, AcceptedAt, DeclinedAt, DeclineReason).

## Status
`DRAFT → PENDING_APPROVAL → APPROVED → SENT → ACCEPTED/DECLINED/EXPIRED/WITHDRAWN`.

## Compensation sensitivity
Salary/offer detail requires stricter permission than candidate profile (`CanViewOfferAsync` — HR/Admin or hiring manager only). Interviewers do NOT see salary.

## No payroll calculation
Offer salary is a proposed contract value; payroll rule engine remains separate.

## No e-signature portal
Acceptance is recorded server-side (AcceptedAt/DeclinedAt); no candidate-facing e-signature (deferred).
