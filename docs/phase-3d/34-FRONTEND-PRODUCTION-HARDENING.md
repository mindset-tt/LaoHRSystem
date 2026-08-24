# 34 — Frontend Production Hardening

## Build
- `npm run build` (Next.js standalone) — verify passes.
- `npx tsc --noEmit` typecheck — verify passes.
- `npm run lint` — verify passes.

## Security
- No secrets in client bundle (verify env vars are server-side only).
- API proxy (`src/proxy.ts`) — verify no SSRF exposure.
- CSP + security headers (see 17).

## Findings
- Next.js 16.1.1 has 3 HIGH vulns (see 33) — must upgrade.

## Follow-up
- Upgrade Next.js/postcss/sharp.
- Verify no `NEXT_PUBLIC_*` leaks secrets.
- Add CSP.
- Verify SSR/API routes are not vulnerable to SSRF.
