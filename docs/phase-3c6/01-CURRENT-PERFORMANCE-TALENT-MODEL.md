# 01 — Current Performance / Talent Model

Audit of performance/talent concepts in the repository.

## Search result
No existing entities for: Performance, Review, Goal, Objective, OKR, Competency, Skill, Feedback, OneOnOne, Training, Course, Learning, Certification, DevelopmentPlan, Career, Talent, Succession.

## Reusable existing entities
| Entity | Reuse for |
|---|---|
| `Employee` | Goal owner, review subject, feedback parties, 1:1 participants, training enrollee |
| `Position` | Position competency requirements, career path |
| `Department` | Organization context |
| `ManagerId` (Employee) | Manager scope (direct reports) |
| `NotificationService` | Performance notifications |
| `AnalyticsService` | KPI extension |
| `IDataScopeService` | Manager/HR/self scope |

## Missing (to build)
- Goal + check-in
- Performance cycle + review (self/manager)
- Continuous feedback + recognition
- 1:1 meeting + action items
- Competency framework + category + position requirements + assessment + gap
- Development plan + goal + action
- Learning catalog + session + enrollment
- Certification
- Career interest + path + readiness
- Talent review foundation

## Decisions
- No duplicate Employee identity (no TalentEmployee/PerformanceEmployee).
- Position reused for competency requirements + career path.
- ManagerId reused for scope.
- No AI scoring/ranking/promotion/firing.
- No forced ranking / 9-box as decision engine.
- No SCORM/xAPI.
