# 03 — Analytics Architecture

## Query layer
`IAnalyticsService` / `AnalyticsService` — aggregates in the database (no N+1), always within the current user's authorized scope.

## DTOs (never EF entities)
`KpiCard`, `TimeSeriesPoint`, `BreakdownItem`, `AttentionItem`, and dashboard payloads (`ExecutiveDashboard`, `HrDashboard`, `ManagerDashboard`, `AttendanceAnalytics`, `LeaveAnalytics`, `PayrollAnalytics`, `FinanceAnalytics`, `PmAnalytics`).

## Endpoints (`AnalyticsController`)
- `GET /api/analytics/executive` — Admin/HR
- `GET /api/analytics/hr` — Admin/HR
- `GET /api/analytics/my-team` — any user (scope = self + direct reports)
- `GET /api/analytics/attendance` — scoped
- `GET /api/analytics/leave` — scoped
- `GET /api/analytics/payroll` — Admin/HR (aggregate only)
- `GET /api/analytics/finance` — scoped
- `GET /api/analytics/pm` — org

## No data warehouse
PostgreSQL + optimized queries. No Snowflake/BigQuery/ClickHouse/Kafka/Spark/OLAP. Materialized views deferred until measured need.

## No cache
No Redis. If dashboard cost becomes meaningful, a short in-memory cache with scope-sensitive keys may be added later.

## N+1 prevention
All aggregations use `GroupBy`/`Sum`/`Count` in a single query per metric group.
