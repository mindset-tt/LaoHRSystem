# 10 — Data Integrity

## Existing integrity mechanisms
- FK constraints (self-referencing Restrict for Department/Employee/ProjectTask).
- Unique constraints (EmployeeCode, Project.Code, TaskAssignee, InterviewParticipant, InterviewEvaluation, CareerInterest).
- Cycle prevention (Department hierarchy, Employee reporting, ProjectTask parent, TaskDependency) — tested.

## Business invariants (tested)
- Hire conversion idempotency (no duplicate Employee).
- Approval terminal-state (no double approve/reject).
- Review manager snapshot (no silent reviewer rewrite).
- Duplicate application prevention.

## Follow-up (not yet implemented)
- Read-only diagnostic command for orphan/invalid references.
- Automated integrity check script against real PostgreSQL.

## No destructive auto-repair
No automatic destructive repair of integrity violations.
