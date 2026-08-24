# 06 — MSS Team Attendance

## Endpoint
`GET /api/team/attendance?date=&from=&to=`

## Scope
Direct reports only, resolved server-side from the JWT.

## Fields
employee name, date, clockIn, clockOut, status, isLate, isEarlyLeave, workHours.

## Privacy
No salary, bank, tax, documents, or medical information exposed.

## No new overtime logic
Only existing `IsLate`/`IsEarlyLeave`/`WorkHours` fields are surfaced; no new statutory overtime calculation.
