# 17 — Certifications

## Model
`EmployeeCertification` (EmployeeId, Name, Issuer, IssuedDate, ExpiryDate, Status).

## Status
`ACTIVE`, `EXPIRED`, `REVOKED`.

## Expiry
ExpiryDate enables expiry-warning notifications (no regulatory meaning unless verified).

## Documents
Reuse existing document architecture for certificate/evidence (no separate binary storage).
