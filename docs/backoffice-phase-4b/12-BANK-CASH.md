# 12 — Bank / Cash

## Entity
`BankAccount`: BankName, AccountName, AccountNumber, Currency, Branch, Swift,
IsActive, OpeningBalance, GLAccountId.

## Security
Full account number is Finance/Admin only (`CanViewBankAccounts`). No masked
representation implemented yet (documented follow-up).

## No bank API
No BCEL/bank transfer API, no automatic payment upload (official technical format
not verified). The existing payroll bank-format blocker remains separate.

## Cash account
No separate `CashAccount`/`TreasuryAccount` abstraction yet (avoid premature
over-abstraction). `BankAccount` is the treasury foundation.

## Access
`CanViewBankAccounts`/`CanManageBankAccounts` (Admin/Finance only).
