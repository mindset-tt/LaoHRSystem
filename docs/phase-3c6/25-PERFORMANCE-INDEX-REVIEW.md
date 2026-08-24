# 25 — Performance / Index Review

## Added indexes (justified by access patterns)
- `Goal (EmployeeId)`, `(Status, DueDate)`.
- `GoalCheckIn (GoalId, CreatedAt)`.
- `PerformanceReview (EmployeeId)`, `(CycleId, Status)`.
- `Feedback (ToEmployeeId)`.
- `OneOnOne (ManagerEmployeeId, EmployeeId)`.
- `PositionCompetency (PositionId)`.
- `CompetencyAssessment (EmployeeId, CompetencyId)`.
- `DevelopmentPlan (EmployeeId)`.
- `DevelopmentGoal (DevelopmentPlanId)`.
- `TrainingEnrollment (EmployeeId)`, `(SessionId, Status)`.
- `EmployeeCertification (EmployeeId)`, `(ExpiryDate)`.
- `CareerInterest (EmployeeId)` unique.
- `TalentReview (EmployeeId)`.

## N+1 prevention
List endpoints use single queries with `Where` scope filters (no per-employee loops).

## No fabricated EXPLAIN
Real PostgreSQL unavailable; no fabricated query plans.
