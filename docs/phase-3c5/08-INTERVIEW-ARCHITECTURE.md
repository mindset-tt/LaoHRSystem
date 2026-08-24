# 08 — Interview Architecture

## Model
`Interview` (ApplicationId, InterviewType, ScheduledStart, ScheduledEnd, Location, Status, OrganizerEmployeeId) + `InterviewParticipant` (EmployeeId, Role).

## Types
`Phone`, `HR`, `HiringManager`, `Technical`, `Panel`, `Final`.

## Status
`SCHEDULED`, `COMPLETED`, `CANCELLED`, `NO_SHOW`.

## Panel authorization
Panel member sees only the assigned interview (not full recruiter access). `CanViewInterviewAsync` checks organizer/participant/application scope.

## Timezone
ScheduledStart/End are explicit; no silent browser-local assumption.

## No calendar integration
No Google/Microsoft calendar connector (deferred).
