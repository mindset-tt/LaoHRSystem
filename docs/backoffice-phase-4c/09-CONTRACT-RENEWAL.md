# 09 — CONTRACT RENEWAL

## Design

`ContractLifecycleService.RenewAsync` records a `ContractHistory` (RENEWAL) with
prior and new terms, then updates the contract. Prior term is never overwritten.

## Invariant

`EndDate >= StartDate` (rejected otherwise).

## Status

PASS.
