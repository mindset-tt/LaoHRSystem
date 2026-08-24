# 01 — Test Determinism

## Observed flake
`LeaveControllerTests.CreateLeave_ValidRequest_ReturnsCreated` intermittently failed with a non-`Created` status (e.g. `BadRequest` from insufficient leave balance or overlapping dates).

## Root cause
`Program.cs` registered the EF Core InMemory database with a **shared static name** `"InMemoryDbForTesting"`. The InMemory provider keys its store by name, so every `WebApplicationFactory` instance (and therefore every parallel xUnit test collection) mutated the **same** in-memory database. Leave requests created by one test leaked into another, causing balance/overlap failures.

## Fix
`CustomWebApplicationFactory` now assigns a **unique database name per factory instance** (`LaoHRTestDb_{Guid}`) and re-registers `DbContextOptions<LaoHRDbContext>` with that name. Each test collection gets fully isolated mutable state.

## Why the fix is correct
- It addresses the actual shared-state race rather than masking it (no `Thread.Sleep`, no global parallelism disable, no test skip).
- It preserves the existing `Program.cs` Testing branch (which still uses a default name) — the factory overrides it at the DI level.
- It is the standard `WebApplicationFactory` pattern for per-fixture database isolation.

## Repeated results
- `CreateLeave_ValidRequest` × 20 consecutive runs: 0 failures.
- Full suite × 5 consecutive runs: 0 failures.

## Full-suite repeated results
5/5 green (104 tests after Phase 3C2B additions).
