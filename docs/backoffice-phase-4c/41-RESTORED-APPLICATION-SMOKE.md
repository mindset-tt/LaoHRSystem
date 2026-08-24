# 41 — RESTORED APPLICATION SMOKE

Phase 4C.1 runs the previously-NOT-RUN application restore smoke.

## Test

`Pg16RestoreSmokeTests.RestoredDb_CorporateEntities_DeserializeAndResolve`
connects the application's `LaoHRDbContext` to the RESTORED PostgreSQL 16
database (`LAOHR_RESTORE_CONNECTION`) and queries every corporate entity,
proving the restored data deserializes and relationships resolve through the
real application data layer.

## Result

PASS — 1/1. All entities (Document+versions, Contract+history, ServiceRequest,
Facility+Room+Booking, WorkOrder, Vehicle+Trip, Travel+Expense, Visitor+Visit)
resolved with correct values.

## Status

PASS.
