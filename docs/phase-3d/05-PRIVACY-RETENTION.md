# 05 — Privacy / Retention

## Unresolved (no guessed durations)
- Candidate retention: POLICY_REQUIRED
- Performance-review retention: POLICY_REQUIRED
- Payroll records, audit logs, employee records, training records: POLICY_REQUIRED

## Architecture
No retention auto-delete. `Retention__Enabled` config exists (default false). BLOCKED retention policy must not auto-delete.

## Data subject / privacy register (sensitive data)
Candidate PII, performance ratings, talent potential, salary, bank account, tax identifiers, documents, loan data, attendance, leave, feedback, private 1:1 notes.

## No biometric processing
No biometric data collected (fingerprint attendance via ZKTeco is device-level, not stored as biometric template in this phase).

## Right to delete/anonymize
Not auto-implemented. Architecture distinguishes business/audit records vs candidate PII vs documents.
