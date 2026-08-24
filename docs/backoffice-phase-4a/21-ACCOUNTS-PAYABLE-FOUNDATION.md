# 21 — Accounts Payable Foundation

## Status
DEFERRED (Phase 4B). No `SupplierInvoice`/`Payment` entities in Phase 4A.

## Planned shape
- `SupplierInvoice`: InvoiceNumber, SupplierId, POId, InvoiceDate, DueDate,
  Currency, Subtotal, Tax, Total, Status (DRAFT, PENDING_VERIFICATION, APPROVED,
  PARTIALLY_PAID, PAID, VOID).
- `Payment`: SupplierInvoiceId, PaymentDate, Amount, Currency, PaymentMethod,
  BankAccountId, ReferenceNumber, Status.

## Three-way match
PO ↔ Goods Receipt ↔ Supplier Invoice. Flag discrepancies; do not silently
approve mismatch.

## Security
Corporate bank account info restricted to Finance/Admin.

## Invariants (future)
- Cannot pay more than invoice remaining balance.
- Posted journal must balance (Debit == Credit).
