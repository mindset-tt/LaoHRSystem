# 26 — CORPORATE AUTHORIZATION

## Capability model

`ICorporateOperationsAccessService` maps roles to capabilities:

| Capability | Admin | HR | Finance | Employee |
|---|---|---|---|---|
| DMS view/manage | ✓ | ✓ | ✓ | — |
| Contract view/manage | ✓ | ✓ | ✓ | — |
| Contract approve | ✓ | ✓ | — | — |
| Service desk manage | ✓ | ✓ | — | — |
| Facility view | ✓ | ✓ | ✓ | — |
| Facility manage | ✓ | ✓ | — | — |
| Room book | ✓ | ✓ | ✓ | ✓ |
| Work order manage | ✓ | ✓ | — | — |
| Fleet view | ✓ | ✓ | ✓ | — |
| Fleet manage | ✓ | ✓ | — | — |
| Vehicle book | ✓ | ✓ | ✓ | ✓ |
| Travel create | ✓ | ✓ | ✓ | ✓ |
| Travel approve | ✓ | ✓ | — | — |
| Travel manage | ✓ | ✓ | ✓ | — |
| Visitor manage | ✓ | ✓ | — | — |
| Corporate report view | ✓ | ✓ | ✓ | — |

Admin is a superset; operational users do not need system Admin.

## Status

PASS.
