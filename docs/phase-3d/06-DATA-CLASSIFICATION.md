# 06 — Data Classification

| Category | Sensitivity | Access scope | Retention |
|---|---|---|---|
| Employee master (name, contact) | Internal | HR/Admin; self | POLICY_REQUIRED |
| Salary / compensation | High | HR/Admin; payroll | POLICY_REQUIRED |
| Bank account | High | HR/Admin; payroll | POLICY_REQUIRED |
| Tax ID / NSSF ID | High | HR/Admin; payroll | POLICY_REQUIRED |
| Candidate PII | High | Recruiter/HM/HR | POLICY_REQUIRED |
| Performance ratings | High | Self/Manager/HR | POLICY_REQUIRED |
| Talent potential | High | HR only | POLICY_REQUIRED |
| Private 1:1 notes | High | Manager only | POLICY_REQUIRED |
| Documents (ID/bank/tax) | High | HR/Admin | POLICY_REQUIRED |
| Attendance | Internal | Self/Manager/HR | POLICY_REQUIRED |
| Leave | Internal | Self/Manager/HR | POLICY_REQUIRED |
| Audit logs | High (integrity) | HR/Admin | POLICY_REQUIRED |
| Notifications | Internal | Self | POLICY_REQUIRED |

## Principle
Sensitive fields are not returned through employee-facing DTOs unnecessarily. Talent/potential is HR-restricted. Manager-private notes never returned to employee.
