# 02 — Auth / Seed Correction

## Defect
Seeded demo user `hr` (2 characters) could never authenticate because `LoginRequestValidator` enforces `Username.Length(3, 50)`.

## Resolution
Renamed the seeded HR demo account from `hr` → `hradmin` (password unchanged `hr123`). This keeps the username policy intact (no weakening of validation) and makes the seeded credential actually usable.

## Username policy ADR
- Minimum username length: **3** characters (unchanged).
- 2-character usernames are **not** allowed.
- Seed naming: `admin`, `hradmin`, `employee` (all ≥ 3 chars).
- Development seed accounts are clearly development-only (seeded only in `IsDevelopment || IsTesting`).

## Seed credential consistency
Added `Login_SeededAccounts_AllAuthenticate` theory test asserting `admin`, `hradmin`, and `employee` all authenticate with their expected roles.

## No password documentation leak
Seed credentials remain development-only. Production does not auto-seed predictable accounts (guarded by `isDevelopment || isTesting`).
