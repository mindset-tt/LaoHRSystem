# 06 — Supplier Master

## Entity
`Supplier` (in `BackOfficeEntities.cs`): SupplierCode, Name, LegalName, TaxId,
RegistrationNumber, Phone, Email, Address, Country, ProvinceId, DistrictId,
BankName, BankAccount, PaymentTerms, Status.

## Security
- Bank + tax fields are sensitive. `SuppliersController.GetSupplier` returns
  `TaxId`/`RegistrationNumber`/`BankName`/`BankAccount` only to Admin/HR.
- Ordinary employees cannot list suppliers at all (`CanViewProcurement` gate).

## Status
`ACTIVE`, `INACTIVE`, `BLOCKED`. Suppliers with purchase orders are never
physically deleted (soft-deactivate via status).

## Numbering
`SUP-{yyyy}-{000000}` via `NumberSequenceService` (concurrency-safe).

## Documents
Reuse existing document infrastructure for contracts/registration/tax
certificates (deferred — no new binary storage).
