# 21 — TRAVEL EXPENSE INTEGRATION

## Design

Reuses the existing `Expense` domain. `Expense.TravelRequestId` links an expense
to a travel request. No `TravelExpense` entity.

## Advance

`TravelAdvance` is distinct from `EmployeeLoan`; it links to `TravelRequest` and
settles against expenses. No separate accounting engine — financial posting flows
through the existing Expense/Payment architecture.

## Policy

No invented per-diem, hotel caps, mileage rates, or flight classes (company policy
only).

## Status

PASS.
