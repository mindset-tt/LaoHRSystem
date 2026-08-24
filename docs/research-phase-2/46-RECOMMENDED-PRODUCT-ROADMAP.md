# 46 — Recommended Product Roadmap + 47 — Recommended Technical Roadmap

> Research date: 2026-08-21. Organized into phases. Do NOT implement — this is research output.

## Product roadmap (by phase)

### PHASE 3A — Correctness & Security
| Item | Priority | Evidence |
|---|---|---|
| Fix ambiguous employee routes | P0 | handoff BR-01 |
| Verify PIT brackets match law | P0 | `04` |
| Verify NSSF ceiling + rates | P0 | `05` |
| Verify seeded LeavePolicy values | P0 | `06` |
| Migrate password hashing SHA-256→Argon2id | P0 | `27` |
| Generate PG migrations (replace EnsureCreated) | P0 | `29` |
| Rotate prior committed credentials | P0 | handoff BR-03 |
| Fail-fast if Jwt:Key empty in Production | P0 | handoff TD-28 |

### PHASE 3B — Production Readiness
| Item | Priority | Evidence |
|---|---|---|
| Add reverse proxy (Caddy) + TLS | P1 | `31` |
| PostgreSQL WAL + PITR backups | P0 | `32` |
| Global exception handler (IExceptionHandler) | P1 | `27` |
| Align CI branch triggers (master) | P1 | handoff |
| Add tests for PM/Finance/Knowledge controllers | P0 | `34` |
| Add frontend tests (Vitest + RTL) | P1 | `34` |
| Remove frontend mock data | P1 | handoff |
| Create /403 page | P1 | handoff |
| Implement proxy.ts redirect (or confirm not needed) | P1 | `27` |
| File upload magic-byte validation | P1 | `27` |
| SMTP real config | P1 | handoff |

### PHASE 3C — Lao Compliance & Localization
| Item | Priority | Evidence |
|---|---|---|
| Add OvertimeEntry (day-type: normal/rest/holiday/night) | P1 | `02`, `03` |
| Split allowances into taxable/non-taxable | P1 | `04` |
| Add personal allowance + dependant deductions | P1 | `04` |
| Auto-link loan installments to payroll deductions | P1 | `23` |
| Minimum wage validation | P1 | `03` |
| PIT monthly withholding report | P1 | `04`, `26` |
| NSSF monthly remittance report (enhance existing) | P2 | `05`, `26` |
| Port address seed SQL to PostgreSQL | P1 | `08`, handoff |
| Add foreign employee fields (nationality, work permit, visa, expiry) | P2 | `10`, `25` |
| Buddhist Era date display (if compliance forms need it) | P2 | `08` |
| NFC normalization for Lao text | P2 | `09` |
| ICU collation for Lao sorting | P2 | `09` |
| Annual holiday import (CSV/Excel) | P2 | `06` |

### PHASE 3D — UX / Self Service
| Item | Priority | Evidence |
|---|---|---|
| Add ManagerId to Employee (reporting line) | P1 | `07`, `12` |
| Department hierarchy (ParentDepartmentId) | P1 | `07` |
| Audit log UI | P2 | `26` |
| User management admin UI | P2 | `11` |
| Charts (lightweight SVG) | P2 | `26`, `50` |
| Role-based dashboards | P2 | `49` |
| Mobile responsive nav drawer | P2 | `16` |
| Error boundaries | P2 | handoff |
| Accessibility (WCAG 2.2 AA: focus, target size, accessible auth) | P2 | `27` |
| ESS: bank details, certificate downloads | P2 | `12` |
| MSS: team dashboard | P2 | `12` |
| Command palette / global search | P2 | `11` |
| PWA (responsive + offline attendance) | P2 | `65`, `66` |

### PHASE 3E — HR Capability Expansion
| Item | Priority | Evidence |
|---|---|---|
| EmploymentType + ProbationEndDate | P2 | `07`, `10` |
| Termination/severance model | P2 | `02`, `07` |
| Onboarding/offboarding task checklist | P2 | `14` |
| Performance management (goals + reviews) | P2 | `15` |
| Position entity + job architecture | P2 | `07` |
| Cost center | P2 | `07`, `23` |
| EmployeeJobHistory (promotion/transfer log) | P2 | `07` |
| Document expiry tracking | P2 | `25` |
| Shifts + breaks + missing punch | P2 | `16` |
| Approval engine (Stateless + Hangfire) | P1 | `17` |
| Bulk employee import (Excel/CSV) | P1 | `11` |
| Accounting export (journal entries) | P2 | `23`, `68` |

### PHASE 3F — PM / PL Capability Expansion
| Item | Priority | Evidence |
|---|---|---|
| Task dependencies (FS/SS/FF/SF) | P2 | `18` |
| Gantt (lightweight CSS-grid) | P2 | `18`, `20` |
| Timeline view | P2 | `18` |
| Task StartDate + EstimatedHours | P2 | `20` |
| Tags/Labels | P2 | `18` |
| Timesheet | P1 | `18`, `21` |
| Resource capacity + utilization | P2 | `21` |
| Risk matrix visualization (SVG) | P2 | `22` |
| RAID log (assumptions + decisions) | P2 | `22` |
| Project health + portfolio view | P2 | `18` |
| Epic/Story/Subtask hierarchy | P2 | `18` |

### PHASE 3G — Analytics & Reporting
| Item | Priority | Evidence |
|---|---|---|
| PIT/NSSF liability reports | P1 | `26` |
| Headcount/turnover reports | P2 | `26` |
| Payroll cost reports | P2 | `26` |
| Attendance/overtime reports | P2 | `26` |
| SVG chart library (line/bar/donut) | P2 | `50` |
| Executive dashboard | P2 | `49` |
| Report builder | P3 | `26` |

### PHASE 3H — Integrations
| Item | Priority | Evidence |
|---|---|---|
| Accounting integration (journal export) | P2 | `68` |
| Webhook framework | P3 | `69` |
| Email notifications (real SMTP) | P1 | `47` |
| In-app notifications | P1 | `47` |
| SMS (if reliable Lao provider found) | P3 | `47` |

### PHASE 3I — AI / Automation
| Item | Priority | Evidence |
|---|---|---|
| RAG knowledge assistant (pgvector) | P3 | `35`, `78` |
| Lao OCR (pilot only) | P3 | `36` |
| Translation assistance | P3 | `36` |
| **Verdict**: Defer all AI until Lao model quality validated + customer demand | — | `35` |

### PHASE 3J — Scale
| Item | Priority | Evidence |
|---|---|---|
| Cursor pagination for large tables | P1 | `29` |
| AsNoTracking + projection DTOs | P2 | `29` |
| Audit log partitioning (monthly) | P2 | `29` |
| Background export queue | P2 | `29` |
| RowVersion optimistic concurrency | P2 | `29` |
| PgBouncer (if multi-instance) | P3 | `29` |
| Read replicas (50k+ scale) | P3 | `29` |
| Materialized views for dashboards | P3 | `29` |

## Technical roadmap (architecture changes)

| Change | Priority | Justification | Evidence |
|---|---|---|---|
| Argon2id password hashing | P0 | OWASP; SHA-256 unsuitable | `27` |
| PG-compatible migrations | P0 | Schema drift | `29` |
| WAL + PITR backups | P0 | Production data safety | `32` |
| Caddy reverse proxy + TLS | P1 | HTTPS in production | `31` |
| IExceptionHandler + ProblemDetails | P1 | RFC 7807 error responses | `27` |
| Stateless + Hangfire | P1/P2 | Approval engine | `17` |
| pg_trgm + ICU collation | P2 | Lao search/sort | `09`, `29` |
| NFC normalization | P2 | Lao text consistency | `09` |
| Audit partitioning | P2 | Unbounded growth | `29` |

## What should NOT be added (KEEP LIGHTWEIGHT)

| Rejected | Reason | Evidence |
|---|---|---|
| Kubernetes | Overkill for 50–5k employees | `31` |
| Elasticsearch | pg_trgm + PG sufficient | `09`, `29` |
| Redis | MemoryCache sufficient at current scale | `58` |
| RabbitMQ/Kafka | No messaging need | `59` |
| Camunda/Elsa BPM | Stateless sufficient | `17` |
| Microservices | Modular monolith is right | `04-ARCHITECTURE` |
| Chart library (recharts) | SVG charts sufficient | `50` |
| State library (Redux/Zustand) | Context sufficient | `03-TECH-STACK` |
| Native mobile app | PWA first | `65` |
| AI/LLM in production | Lao quality unvalidated | `35`, `36` |
| Multi-tenancy SaaS | No customer demand | `56` |