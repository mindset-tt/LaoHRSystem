# 07 — Migration Inventory

| Migration | Purpose | Tables | Destructive | Rollback risk |
|---|---|---|---|---|
| InitialCreatePostgres | Baseline schema | All core + org + approval | No | High (baseline) |
| AddApprovalEssMssNotifications | Notifications + attendance corrections | Notifications, AttendanceCorrections | No | Low |
| AddPmPlanningAndDependencies | PM planning | TaskDependencies, ProjectAssumptions, ProjectDecisions + ParentTaskId | No | Low |
| AddPmPlanningDependenciesV2 | No-op (seed timestamp refresh) | None | No | None |
| AddRecruitmentAndOnboarding | Recruitment | 12 tables | No | Low |
| AddPerformanceTalentLearning | Performance/talent/learning | 17 tables | No | Low |

## Validation (real PostgreSQL 16)
- Fresh: 6 migrations → 84 tables. PASS.
- Upgrade: partial (2 migrations) → full (6 migrations). PASS.
- Restore: pg_dump custom → clean restore → 84 tables. PASS.

## No rebase
No migration regenerated. No-op migration preserved (historical infrastructure).

## Design-time factory fix
`LaoHRDbContextFactory` now sets `Npgsql.EnableLegacyTimestampBehavior` (was missing, causing "Unspecified DateTime" failure on `dotnet ef database update`).
