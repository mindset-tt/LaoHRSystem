# 12 — Project Health

## Dimensions
- **Schedule Health**: ON_TRACK / AT_RISK / DELAYED (documented rules).
- **Risk Health**: GREEN / AMBER / RED (RED if critical open risk, AMBER if any open risk).
- **Overall Health**: RED if any dimension red, AMBER if any amber, else GREEN.

## No magic score
Deterministic rules, no hidden AI. Dimensions exposed separately.

## Endpoint
`GET /api/pm/projects/{id}/health`.
