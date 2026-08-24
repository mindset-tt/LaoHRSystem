# 03 — Approval Security

## Regression suite added (`ApprovalServiceTests`)
- `Approve_AfterTerminalApproval_Throws` — second approval on APPROVED request rejected.
- `Approve_AfterRejection_Throws` — approval after REJECTED rejected.
- `Reject_AfterTerminalApproval_Throws` — reject after APPROVED rejected.
- `ApproverSnapshot_ManagerChange_DoesNotAlterPendingRequest` — pending request stays with original approver after manager change.
- `NewRequest_AfterManagerChange_ResolvesNewManager` — new request resolves to new manager.

## Approval detail authorization (IDOR fix)
`ApprovalsController.GetApproval` previously returned any approval request to any authenticated user. Now restricted to:
- the requester,
- a current approver (any step),
- HR/Admin.

Unrelated users receive `403 Forbid`.

## My Requests
Added `GET /api/approvals/mine` returning the current user's submitted requests (consolidated ESS "My Requests" view).

## Approver resolution determinism
`ROLE` resolver selects the first active employee with the role ordered by `UserId` (deterministic, not random). Documented in `07-APPROVAL-ARCHITECTURE.md`.
