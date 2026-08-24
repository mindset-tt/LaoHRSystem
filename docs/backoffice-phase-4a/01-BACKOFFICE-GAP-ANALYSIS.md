# 01 — Back Office Gap Analysis

## Method
Audited the entire repository (entities, controllers, services, DbContext,
frontend routes/components/lib) against the target Back Office domain map.

## Gap matrix

| Module | Existing | Missing | Reuse | Priority | Implementation |
|---|---|---|---|---|---|
| HR | Full | — | — | — | Preserved |
| Finance | Expense, Loan | Budget, AP, AR, GL, Cash/Bank | Expense/Approval | P0/P1 | Budget done; AP/AR/GL deferred |
| Procurement | — | PR, PO, Goods Receipt | ApprovalService, NotificationService | P0 | Done |
| Supplier | — | Supplier master | EntityComment, Documents | P0 | Done |
| Inventory | — | Item, Category, Warehouse, Stock ledger | — | P0 | Done |
| Warehouse | — | Warehouse | WorkLocation | P0 | Done |
| Assets | — | Asset, Assignment | InventoryCategory | P0 | Done |
| Contracts | — | Contract register | Documents, Notification | P1 | Done |
| Projects | Full | — | — | — | Preserved |
| Documents | EmployeeDocument | Platform DMS | — | P1 | Deferred (reuse existing) |
| Internal Requests | — | ServiceRequest | ApprovalService | P1 | Done |
| Accounting | — | CoA, Journal, FiscalPeriod | — | P2 | Deferred |

## Key findings
1. **No ERP entities existed** — all 19 Back Office entities are new.
2. **Approval engine is reusable** — `ApprovalService` is polymorphic; new
   `PURCHASE_REQUEST` type added without a new workflow engine.
3. **Numbering was not concurrency-safe** — replaced with `NumberSequence` table.
4. **Only 3 roles** — added `BackOfficeAccessService` to map roles to module
   capabilities without a new permission table (future-proof).
5. **Audit is automatic** — `AuditLogInterceptor` covers all new entities.

## What should NOT be built yet (deferred)
- Full Accounts Payable / Receivable, General Ledger, Financial Statements.
- Depreciation engine, advanced budget planning, supplier portal, multi-company.
- OCR, AI, RAG, microservices, CI/CD, cloud deployment.
