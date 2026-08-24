# 23 — Back Office Navigation

## Sidebar reorganization
`Sidebar.tsx` extended with logical module groups (permission-gated):

- **HOME** — Dashboard
- **PEOPLE** — Employees, Recruitment, Performance, Attendance, Leave, Payroll
- **FINANCE** — Expenses/Loans (existing), Budgets (new)
- **PROCUREMENT** — Purchase Requests, Purchase Orders, Goods Receipts, Suppliers
- **INVENTORY** — Items, Warehouses, Stock
- **ASSETS** — Asset Register
- **CONTRACTS** — Contracts
- **INTERNAL REQUESTS** — Service Requests
- **PROJECTS** — Portfolio, Projects, My Tasks, Capacity
- **REPORTS / ANALYTICS / ADMIN** — existing

## Permissions
- `procurement.view`, `inventory.view`, `assets.view`, `contracts.view` → Admin/HR.
- `finance.view` → Admin only.
- Service Requests → all (self-service).

## UX note
Hiding a menu item is UX only; API authorization remains server-side.
