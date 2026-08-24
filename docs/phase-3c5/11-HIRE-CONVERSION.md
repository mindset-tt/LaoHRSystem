# 11 — Hire Conversion

## Service
`IHireConversionService` / `HireConversionService` — the single controlled candidate → Employee boundary.

## Responsibilities
- Validate application + accepted offer.
- Prevent duplicate employee (email check).
- Map safe person data (name, email, phone).
- Map Position/Department/WorkLocation/Manager from requisition.
- Set employment dates + salary (from offer).
- Create onboarding process.
- Mark application + candidate HIRED.

## Idempotency
Calling twice does NOT create two Employees (returns existing). Tested.

## Duplicate prevention
Rejects if an employee with the same email already exists (HR confirmation required).

## Employee number
Reuses existing `EMP{count:D4}` convention (no second numbering system).

## No copy-everything
Interview notes are NOT copied into Employee. Recruitment history stays linked (ApplicationId/CandidateId on onboarding) for authorized review.

## Transactional
Employee creation + application hire-state + onboarding creation in one service flow.
