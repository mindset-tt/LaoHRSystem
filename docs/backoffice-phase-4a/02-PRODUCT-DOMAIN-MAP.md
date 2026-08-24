# 02 — Product Domain Map

```
BACK OFFICE
│
├── People
│   ├── HR            ✅ existing
│   ├── Attendance    ✅ existing
│   ├── Leave         ✅ existing
│   ├── Payroll       ✅ existing (NSSF/PIT)
│   ├── Recruitment   ✅ existing
│   ├── Performance   ✅ existing
│   └── Learning      ✅ existing
│
├── Finance
│   ├── Expenses      ✅ existing
│   ├── Budgets       ✅ Phase 4A
│   ├── Accounts Payable   ⏳ deferred (foundation only)
│   ├── Accounts Receivable ⏳ deferred
│   ├── Cash / Bank   ⏳ deferred
│   └── Financial Reporting ⏳ deferred
│
├── Procurement
│   ├── Purchase Request   ✅ Phase 4A
│   ├── Approval           ✅ Phase 4A (reuses ApprovalService)
│   ├── RFQ foundation     ⏳ deferred
│   ├── Purchase Order     ✅ Phase 4A
│   ├── Goods Receipt      ✅ Phase 4A
│   └── Supplier           ✅ Phase 4A
│
├── Inventory
│   ├── Item               ✅ Phase 4A
│   ├── Category           ✅ Phase 4A
│   ├── Warehouse          ✅ Phase 4A
│   ├── Stock              ✅ Phase 4A (ledger-derived)
│   ├── Movement           ✅ Phase 4A
│   ├── Transfer           ✅ Phase 4A
│   └── Adjustment         ✅ Phase 4A
│
├── Assets
│   ├── Fixed Asset        ✅ Phase 4A
│   ├── Assignment         ✅ Phase 4A
│   ├── Maintenance        ⏳ deferred
│   ├── Depreciation       ⏳ deferred (foundation only)
│   └── Disposal           ✅ Phase 4A
│
├── Projects               ✅ existing (full)
│
├── Corporate Operations
│   ├── Contracts          ✅ Phase 4A
│   ├── Documents          ✅ existing (EmployeeDocument)
│   ├── Internal Requests  ✅ Phase 4A (ServiceRequest)
│   ├── Facilities         ⏳ deferred
│   ├── Office Supplies    ⏳ deferred (use Inventory)
│   └── Knowledge          ✅ existing
│
└── Platform
    ├── Identity           ✅ existing
    ├── RBAC               ✅ existing + BackOfficeAccessService
    ├── Approval           ✅ existing (reused)
    ├── Notification       ✅ existing (reused)
    ├── Audit              ✅ existing (automatic)
    ├── Settings           ✅ existing
    ├── Reporting          ✅ existing
    └── Analytics          ✅ existing
```

Legend: ✅ = implemented, ⏳ = deferred to later phase.
