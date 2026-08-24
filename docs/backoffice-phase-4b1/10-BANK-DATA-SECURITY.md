# 10 — Bank Data Security

## Masking
`BankAccountsController.GetBankAccounts` masks the account number server-side
(last 4 digits) for viewers; full account number + Swift + opening balance only
for `CanManageBankAccounts` (Finance/Admin).

## Supplier bank data
Same pattern applies (Phase 4A.1 already restricted supplier bank/tax fields to
Admin/Finance).

## Logging
No full bank account logged unnecessarily.

## No bank API
No BCEL/bank transfer API, no automatic payment upload (official format not
verified). The payroll bank-format blocker remains separate.
