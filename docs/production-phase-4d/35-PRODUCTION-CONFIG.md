# 35 — PRODUCTION CONFIG

## Configuration matrix

| Setting | Required | Secret | Example | Validation |
|---|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | YES | YES | `Host=postgres;...` | startup |
| `Jwt__Key` | YES | YES | 64+ random chars | startup (≥64) |
| `Jwt__Issuer` / `Jwt__Audience` | YES | NO | `LaoHRServer` / `LaoHRClient` | — |
| `Cors__AllowedOrigins` | YES | NO | `https://app.example.com` | startup |
| `POSTGRES_PASSWORD` | YES | YES | strong random | compose `:?` |
| `NEXT_PUBLIC_API_URL` | YES | NO | `https://api.example.com` | build |
| `ForwardedHeaders__KnownProxy` | optional | NO | proxy IP | — |
| `Serilog__WriteToFile` | optional | NO | `true`/`false` | — |
| `OpenTelemetry__Otlp__Endpoint` | optional | NO | collector URL | — |

## Debug features

Swagger is Development-only. Demo users seeded only in Development/Testing.
No debug endpoints in Production.

## Status

PASS.
