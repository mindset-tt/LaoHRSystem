# 15 — Learning Architecture

## Model
`LearningCourse` (Code, Title, TitleLao, Description, Category, DeliveryType, IsActive) + `TrainingSession` (CourseId, Start, End, Location, Instructor, Capacity, Status) + `TrainingEnrollment` (SessionId, EmployeeId, Status, AssignedByEmployeeId, EnrolledAt, CompletedAt, Result).

## Delivery types
`Classroom`, `Online`, `Workshop`, `External`, `SelfStudy`.

## Enrollment status
`ASSIGNED`, `ENROLLED`, `IN_PROGRESS`, `COMPLETED`, `CANCELLED`, `NO_SHOW`.

## No SCORM/xAPI
No SCORM/xAPI (documented as future LMS enhancement). No course-content authoring.

## No cost accounting
No training cost integration (deferred).

## Authorization
HR/Admin manage catalog/sessions. Manager assigns to direct reports. Employee completes own enrollment.
