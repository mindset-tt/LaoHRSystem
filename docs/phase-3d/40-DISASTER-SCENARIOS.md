# 40 — Disaster Scenarios

## Scenarios
1. DB corruption / accidental delete → restore from backup (validated).
2. Host failure → redeploy from images + restore DB.
3. Ransomware → restore from off-site backup.
4. Credential leak → rotate secrets (see 12).
5. Certificate expiry → renew (see 44).
6. Region/DC outage → DR site failover (not configured).

## Current capability
- DB restore: validated (PASS).
- Off-site backup: NOT configured.
- DR site: NOT configured.

## Follow-up
- Off-site encrypted backup.
- DR site + failover plan.
- Document runbooks (41-46).
