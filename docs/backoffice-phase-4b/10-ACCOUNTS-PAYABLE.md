# 10 — Accounts Payable

## Derivation
AP balance derives from approved supplier invoices and payments. No manually
editable "OutstandingAP" summary number.

## Aging
`AccountsPayableService.GetAgingAsync` buckets RemainingAmount by DueDate:
Current (≤0 days), 1–30, 31–60, 61–90, 90+.

## Invariant
DRAFT / PENDING_MATCH / MATCH_EXCEPTION / PENDING_APPROVAL invoices cannot be paid.

## Access
`CanViewAp` (Admin/Finance only).
