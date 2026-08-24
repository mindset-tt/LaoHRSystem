# 11 — Payments

## Entities
- `Payment`: PaymentNumber, PaymentDate, PaymentMethod, Currency, Amount,
  BankAccountId, ReferenceNumber, Status (DRAFT/PENDING_APPROVAL/APPROVED/POSTED/VOID).
- `PaymentAllocation`: PaymentId, SupplierInvoiceId, Amount (one payment → one or
  many invoices).

## Partial payment
Invoice 100 → Payment 40 → Payment 60 → Remaining 0 → PAID. No overpayment
(no credit/prepayment model yet).

## Idempotency
`PostPaymentAsync` is transition-guarded (only DRAFT/APPROVED can post; POSTED
re-posting throws). Double-posting prevented.

## Concurrency
Payment allocation uses a transaction + row lock (relational) so two simultaneous
payments cannot overpay the same invoice.

## Approval
Reuse `ApprovalService` if policy requires (not yet wired); payment creator ≠
approver is a configurable segregation-of-duties control.

## Access
`CanCreatePayments`/`CanApprovePayments` (Admin/Finance only).
