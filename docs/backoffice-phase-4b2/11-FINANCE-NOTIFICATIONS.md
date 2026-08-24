# 11 — FINANCE NOTIFICATIONS

Finance workflow notifications (best-effort, via `INotificationService`).

## Implemented

| Type | Trigger | Recipient |
|---|---|---|
| `SUPPLIER_INVOICE_MATCH_EXCEPTION` | Invoice match returns a variance | Invoice creator |
| `SUPPLIER_INVOICE_APPROVED` | Invoice approved | Invoice creator |
| `PAYMENT_APPROVED` | Payment approved | Payment creator |

## Data safety

Notifications contain only the invoice/payment number and status — no full bank
account numbers, secrets, or confidential banking information.

## Lifecycle evaluation

- `SUPPLIER_INVOICE_REJECTED` / `PAYMENT_REJECTED`: the current lifecycle has no
  explicit "reject" transition (invoices are voided, not rejected), so these are
  not implemented.
- `SUPPLIER_INVOICE_AWAITING_APPROVAL` / `PAYMENT_AWAITING_APPROVAL`: not
  implemented (no awaiting-approval notification requirement in the current
  workflow).

## Status

PASS — implemented notifications are best-effort and data-safe; unimplemented
types are documented as not applicable to the current lifecycle.
