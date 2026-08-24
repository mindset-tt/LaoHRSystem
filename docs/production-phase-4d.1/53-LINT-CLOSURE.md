# 53 — FRONTEND LINT CLOSURE

Phase 4D carried historical lint debt reported as "41 errors / 39 warnings".
At the start of Phase 4D.1 the actual measured state was:

```
npx eslint .
✔ 50 problems (19 errors, 31 warnings)
```

(The delta vs the historical report is earlier partial fixes.)

## Inventory (from ESLint JSON output)

- `react-hooks/set-state-in-effect` × 14
- `react-hooks/immutability` ("cannot access variable before it is declared") × 4
- `@typescript-eslint/no-explicit-any` × 1
- unused vars/imports × ~14
- `react-hooks/exhaustive-deps` × ~8
- `@next/next/no-location-assign-relative-destination` × 1
- misc

## Rules applied

**No eslint-disable comments were added; no rule severities changed; no files
ignored.** Fix patterns used:

1. set-state-in-effect: synchronous setState inside effect bodies wrapped in
   `React.startTransition(...)` (the codebase's established pattern), or
   declaration-order fixes so loaders are stable references.
2. immutability/use-before-declaration: moved function declarations above the
   effects referencing them.
3. exhaustive-deps: wrapped label helpers in `useCallback` with correct dep
   arrays; added missing stable deps.
4. no-explicit-any: replaced with the existing `'EARNING' | 'DEDUCTION' |
   'BONUS'` union / `Dictionary` types.
5. unused vars: removed only provably-dead code (no behavior change).
6. apiClient location assign: converted to
   `window.location.assign(new URL('/login?expired=true', window.location.origin).href)`
   (same destination, satisfies rule).

## Final verification

```
npx eslint .        → 0 problems (0 errors, 0 warnings)
npx tsc --noEmit    → exit 0
npm test            → 57 passed / 57 (14 files)
npm run build       → PASS (Next.js 16.3.2 production build)
```

Note: completing the type fixes also repaired pre-existing uncommitted tsc
failures introduced by an earlier partial lint session (dashboard/leave
`Record<string, Record<string,string>>` typings) — fixed using the existing
`Dictionary` i18n type; pure type-level change.

## Status

LINT = **PASS** (errors 0, warnings 0).
