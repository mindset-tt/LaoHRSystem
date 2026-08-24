# 02 — Target Architecture

## Principle
ONE Employee identity, ONE Position source of truth, ONE Manager relationship, ONE Notification infrastructure, ONE Analytics architecture. Performance/Talent adds employee-development records (no TalentEmployee/PerformanceEmployee/LearningEmployee).

## New entities (Phase 3C6)
- `Goal` + `GoalCheckIn` — goals/OKR with progress history.
- `PerformanceCycle` + `PerformanceReview` — formal review cycle (manager/position snapshotted).
- `Feedback` — continuous feedback/recognition.
- `OneOnOne` — 1:1 meetings (shared/manager-private/employee notes separated).
- `Competency` + `PositionCompetency` + `CompetencyAssessment` — framework + requirements + assessments.
- `DevelopmentPlan` + `DevelopmentGoal` — IDP.
- `LearningCourse` + `TrainingSession` + `TrainingEnrollment` — learning catalog.
- `EmployeeCertification` — certifications.
- `CareerInterest` — career aspirations.
- `TalentReview` — human-entered performance × potential.

## New service
`IPerformanceAccessService` — contextual access (self/manager/HR).

## New endpoints (`PerformanceController`, `/api/performance`)
Goals (+check-ins), cycles, reviews (self/manager/finalize/acknowledge), feedback, 1:1s, competencies (+position requirements + assessments), development plans, courses, enrollments, certifications, career, talent.

## Reused (not duplicated)
`Employee`, `Position`, `Department`, `ManagerId`, `NotificationService`, `AnalyticsService`, `IDataScopeService`.

## Deferred (not justified)
360 feedback, calibration, succession engine, SCORM/xAPI, course-content authoring, 9-box as decision engine, AI scoring/ranking.
