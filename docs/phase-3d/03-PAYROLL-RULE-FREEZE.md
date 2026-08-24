# 03 — Payroll Rule Freeze

## Decision
PRODUCTION_PAYROLL_READY = **NO**. No statutory calculation rule is guessed or defaulted.

## Verified rules (32 LOCKED)
PIT brackets/dependent/OT-exempt/foreign/withhold/annual/penalty; NSSF employer/employee/ceiling/deadline/foreign/deductible; min wage; OT rates (1.5×/2×/2.5×/3×/15% night); OT caps; leave annual/sick/maternity; probation; notice; severance; foreign quota; work hours.

## Blocked rules (6 + rounding)
OT divisor, leave carry-over, NSSF min floor, NSSF base exact definition (TEMPORARY), unused-leave payout, bank format, visa categories, rounding.

## Fail-closed gate
`ComplianceRuleService.GetEffectiveRuleAsync` excludes BLOCKED rules. Production payroll cannot execute a blocked statutory calculation.

## Operating mode
Non-payroll HR functionality may be deployed while payroll remains disabled. Payroll requires professional confirmation of the 6 items.
