# 09 — Three-Way Match

## Comparison (PO-backed invoice)
PURCHASE ORDER ↔ GOODS RECEIPT ↔ SUPPLIER INVOICE:
- ordered quantity (PO line)
- received quantity (posted GoodsReceiptItems)
- invoiced quantity (invoice line)
- PO unit price vs invoice unit price

## Match status
MATCHED, QUANTITY_VARIANCE, PRICE_VARIANCE, QUANTITY_AND_PRICE_VARIANCE,
NOT_APPLICABLE (non-PO).

## Tolerance
No hardcoded 5% (or any) tolerance. Configurable policy (QuantityTolerancePercent,
PriceTolerancePercent) is a follow-up; default is exact match.

## Override
Authorized Finance/Procurement may resolve variance (reason + user + timestamp,
audited). Not yet wired as a dedicated endpoint (documented follow-up).

## Access
`CanManageAp` (Admin/Finance only).
