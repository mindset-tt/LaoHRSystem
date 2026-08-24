# 27 — Performance Indexes

## Indexes added (Phase 4B)
- `Account` (AccountCode) unique, (ParentAccountId).
- `FiscalPeriod` (FiscalYearId, PeriodNumber) unique.
- `JournalEntry` (JournalNumber) unique, (Status, PostingDate), (FiscalPeriodId), (SourceType, SourceId).
- `JournalLine` (JournalEntryId), (AccountId), (CostCenterId), (ProjectId).
- `SupplierInvoice` (SupplierId, InvoiceNumber) unique, (Status), (DueDate), (PurchaseOrderId).
- `SupplierInvoiceLine` (SupplierInvoiceId).
- `Payment` (PaymentNumber) unique, (Status, PaymentDate).
- `PaymentAllocation` (PaymentId), (SupplierInvoiceId).
- `BankAccount` (AccountNumber).
- `Customer` (CustomerCode) unique.
- `CustomerInvoice` (CustomerId, InvoiceNumber) unique, (Status, DueDate).
- `CustomerInvoiceLine` (CustomerInvoiceId).
- `Receipt` (ReceiptNumber) unique.
- `ReceiptAllocation` (ReceiptId), (CustomerInvoiceId).

## Note
Single-host optimization; PostgreSQL indexes on FK + filter columns.
