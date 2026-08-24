# 08 — Continuous Feedback

## Model
`Feedback` (FromEmployeeId, ToEmployeeId, Type, Message, Visibility, CreatedAt).

## Types
`Feedback`, `Recognition`.

## Visibility
`Private` (recipient only), `RecipientAndManager`.

## Authority
FromEmployeeId is server-resolved (never client-supplied).

## No gamification
No points/leaderboards.

## Edit/delete
Deferred (limited editing window policy).
