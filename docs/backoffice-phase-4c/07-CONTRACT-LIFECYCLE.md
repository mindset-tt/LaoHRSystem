# 07 — CONTRACT LIFECYCLE

## States

DRAFT, UNDER_REVIEW, PENDING_APPROVAL, ACTIVE, EXPIRING, EXPIRED, TERMINATED,
CANCELLED, ARCHIVED (only the states the flow needs are used).

## Flow

DRAFT → submit-approval → PENDING_APPROVAL → approve → ACTIVE → (renew/terminate).

## History

`ContractHistory` is append-only. Renewal/termination preserve prior
start/end/amount/status. The canonical `Contract` row is updated, but history is
never lost.

## API

`ContractsController` extended with `submit-approval`, `approve`, `renew`,
`terminate` (via `ContractLifecycleService`), and `history`.

## Tests

`ContractLifecycleServiceTests` — renewal preserves prior term, end-before-start
rejected, termination preserves prior status.

## Status

PASS.
