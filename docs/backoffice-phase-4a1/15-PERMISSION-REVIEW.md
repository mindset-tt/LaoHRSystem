# 15 — Permission Review

## Change (4A.1)
Supplier bank/tax sensitive fields (`TaxId`, `RegistrationNumber`, `BankName`,
`BankAccount`) are now restricted to **Admin (Finance) only** via
`CanViewSupplierSensitive()`. Previously HR also had access — corrected.

## Capability mapping (no overly-broad BACKOFFICE_ADMIN)
| Capability | Admin | HR | Employee |
|---|---|---|---|
| View/Manage Procurement | ✅ | ✅ | self-service create |
| Approve Procurement | ✅ | ✅ | ❌ |
| View/Manage Finance (budgets) | ✅ | ❌ | ❌ |
| View/Manage Inventory | ✅ | ✅ | ❌ |
| Adjust Inventory | ✅ | ✅ | ❌ |
| View/Manage Assets | ✅ | ✅ | ❌ |
| Assign Assets | ✅ | ✅ | ❌ |
| View/Manage Contracts | ✅ | ✅ | ❌ |
| Manage Service Requests | ✅ | ✅ | self-service create |
| Supplier bank/tax fields | ✅ | ❌ | ❌ |

## Principle
HR is NOT treated as Finance. Domain permissions (FINANCE, PROCUREMENT_ADMIN,
ADMIN) preferred over role-name shortcuts.
