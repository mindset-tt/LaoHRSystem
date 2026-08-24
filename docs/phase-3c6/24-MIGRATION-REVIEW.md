# 24 — Migration Review

## Chain (append-only)
```
InitialCreatePostgres
  → AddApprovalEssMssNotifications
  → AddPmPlanningAndDependencies
  → AddPmPlanningDependenciesV2   (no-op: seed timestamp refresh)
  → AddRecruitmentAndOnboarding
  → AddPerformanceTalentLearning
```

## `AddPerformanceTalentLearning` review
- **New tables**: Goals, GoalCheckIns, PerformanceCycles, PerformanceReviews, Feedbacks, OneOnOnes, Competencies, PositionCompetencies, CompetencyAssessments, DevelopmentPlans, DevelopmentGoals, LearningCourses, TrainingSessions, TrainingEnrollments, EmployeeCertifications, CareerInterests, TalentReviews.
- **No DROP/TRUNCATE/destructive ALTER** in `Up` (all `DropTable` are in `Down`).
- **FK delete behavior**: Performance history does NOT disappear when Employee becomes inactive (no cascade from Employee to Goal/Review/Feedback/etc.). `CareerInterest (EmployeeId)` unique (one per employee).
- **Unique constraints**: `CareerInterest (EmployeeId)` unique.

## No-op migration
`AddPmPlanningDependenciesV2` remains (harmless seed-timestamp refresh, documented).
