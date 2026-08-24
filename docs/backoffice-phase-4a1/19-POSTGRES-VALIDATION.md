# 19 — PostgreSQL Validation

## Canonical target
PostgreSQL 16 (`postgres:16-alpine`).

## Results (disposable container on 127.0.0.1:5434)
| Test | Result |
|---|---|
| Fresh migration (full chain) | PASS — 103 tables |
| Upgrade migration (pre-BO → BO) | PASS — 84 → 103 tables, data preserved |
| Back Office tables (19) | PASS |
| Seed data (ServiceRequestCategories) | PASS (6 rows) |
| Budget/asset columns (ReservedAmount, CommittedAmount, ActualAmount, GoodsReceiptItemId) | PASS |

## Note
Native PostgreSQL 18.6 is NOT the production target. 18 succeeding does not
prove 16 compatibility — hence the dedicated 16 validation.
