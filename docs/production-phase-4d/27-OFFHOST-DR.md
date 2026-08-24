# 27 — OFF-HOST DR

## Design

Simple off-host destination (operator's choice): second local server, NAS, or
encrypted external disk. No forced commercial cloud.

## Copy

Operator-run script copies the backup + document storage to the off-host
destination. Encryption recommended if the backup leaves the host (key not
hardcoded).

## Status

PARTIAL — design documented; off-host copy is an operator step (no automated
off-host sync implemented). Not a P0 code blocker.
