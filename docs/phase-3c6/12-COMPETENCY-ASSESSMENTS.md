# 12 — Competency Assessments

## Model
`CompetencyAssessment` (EmployeeId, CompetencyId, AssessorEmployeeId, AssessmentType, Level, CycleId, AssessedAt).

## Provenance
Who assessed, when, level, source, cycle. No permanent "current skill score" without provenance.

## Self vs manager
`AssessmentType` = Self/Manager (distinguishable, not overwritten).

## Authorization
Self: own. Manager: team. HR: broader. Unrelated: 403.
