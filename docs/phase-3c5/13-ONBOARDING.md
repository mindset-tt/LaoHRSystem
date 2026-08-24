# 13 — Onboarding

## Model
`OnboardingProcess` (EmployeeId, ApplicationId, CandidateId, StartDate, OwnerEmployeeId, Status, CompletedAt) + `OnboardingTask` (Title, Description, OwnerEmployeeId, DueDate, CompletedAt, Category, SortOrder).

## Categories
`HR`, `IT`, `Manager`, `Facilities`, `Employee`.

## Owners
Tasks assigned to actual Employees (no free-text owner identities).

## No dependency graph
Simple checklist ordering (SortOrder) — no sophisticated task dependency graph.

## Not ProjectTask
Onboarding tasks have a different lifecycle/security than ProjectTask; a lightweight `OnboardingTask` is used (not reused ProjectTask).

## ESS/MSS views
"My Onboarding" (employee) and "New Joiners" (manager) — only visible/actionable tasks, no confidential recruitment feedback.
