# 16 — AR Foundation

## Entities (minimal, not full CRM)
- `Customer`: CustomerCode, Name, LegalName, TaxId, Phone, Email, Address, Status.
- `CustomerInvoice`: InvoiceNumber, CustomerId, InvoiceDate, DueDate, Currency,
  Subtotal, TaxAmount, TotalAmount, PaidAmount, RemainingAmount, Status.
- `CustomerInvoiceLine`: Description, Quantity, UnitPrice, Subtotal, TaxAmount, AccountId.
- `Receipt`: ReceiptNumber, ReceiptDate, Currency, Amount, BankAccountId, Status.
- `ReceiptAllocation`: ReceiptId, CustomerInvoiceId, Amount.

## Status
DRAFT, APPROVED, PARTIALLY_PAID, PAID, VOID.

## Posting
AR/receipt posting requires account mappings (Debit AR / Credit Revenue; Debit
Bank / Credit AR) — not auto-posted until mappings exist.

## Scope
Model + APIs + tests only; full customer billing UI is PARTIAL/DEFERRED.
