# 16 — Training Catalog / Sessions

## Catalog
`LearningCourse` is the catalog entry (code, title, description, category, delivery type).

## Sessions
`TrainingSession` is a scheduled instance of a course (start/end, location, instructor, capacity, status).

## Assignment
Manager/HR assign training via `TrainingEnrollment` (AssignedByEmployeeId server-resolved).

## Required training
Required/optional flag deferred (no inferred legal mandatory training).
