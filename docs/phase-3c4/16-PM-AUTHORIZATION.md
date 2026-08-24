# 16 — PM Authorization

## Service
`IProjectAccessService` / `ProjectAccessService`.

## Rules
- **Admin/HR**: full project access.
- **Owner**: full access to own project.
- **LEAD**: edit project, manage tasks/resources/risks.
- **MEMBER**: view, manage tasks, manage risks.
- **VIEWER**: view only.
- **Unrelated employee**: no access (403).

## Methods
`CanViewProjectAsync`, `CanEditProjectAsync`, `CanManageTasksAsync`, `CanManageResourcesAsync`, `CanManageRisksAsync`.

## Separation of concerns
- People-manager ≠ Project Manager.
- Project Manager ≠ HR permissions.
- Scopes kept separate.

## IDOR
`/api/pm/projects/{id}/*` endpoints enforce `CanViewProjectAsync`/`CanManageTasksAsync`/etc. Non-members get 403.

## Tests
`PmAuthorizationTests` (4): non-member cannot view board/timeline/health or add dependency.
