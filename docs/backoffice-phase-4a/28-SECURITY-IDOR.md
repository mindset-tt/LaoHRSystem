# 28 — Security & IDOR

## High-risk domains (explicit authorization)
Finance, Payments, Supplier banking, Inventory adjustments, Assets, Contracts.

## IDOR protection (tested)
`BackOfficeAuthorizationTests` (13 tests) proves:
- Employee cannot list suppliers / purchase orders / inventory items / warehouses
  / assets / contracts / budgets (403 Forbidden).
- Employee CAN create own purchase request + service request (self-service).
- Employee A cannot read Employee B's purchase request / service request (403).
- Employee cannot approve a purchase request (403).
- Employee cannot update a service request (403).

## Supplier sensitive fields
`TaxId`/`RegistrationNumber`/`BankName`/`BankAccount` returned only to Admin/HR.

## Audit
All financial/stock/asset mutations audited automatically via `AuditLogInterceptor`.
Never log secret data.

## Segregation of duties
Requester ≠ final approver (server-side resolution). Creator of payment may
differ from approver (future).
