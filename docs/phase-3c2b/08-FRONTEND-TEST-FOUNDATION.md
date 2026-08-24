# 08 — Frontend Test Foundation

## Stack
Vitest + @testing-library/react + @testing-library/jest-dom + @testing-library/user-event + jsdom + @vitejs/plugin-react.

## Config
- `vitest.config.mts` (jsdom environment, `@` alias, `src/**/*.test.{ts,tsx}`).
- `src/test/setup.ts` (jest-dom matchers, matchMedia polyfill, cleanup).
- `src/test/renderWithProviders.tsx` (LanguageProvider + ToastProvider wrapper).

## Scripts
- `npm test` → `vitest run` (non-interactive, CI-safe).
- `npm run test:watch` → `vitest`.

## Tests (18 passing)
- API clients: `approvalsApi`, `notificationsApi`, `teamApi`, `employeesApi.getMe` (response normalization).
- Components: My Profile, My Approvals, Notifications, My Team (loading/success/empty/error states).

## Gate
18 passed / 0 failed.
