# 27 — Competitor Lessons + 28 — Lao Product Priority Matrix + 29 — AI Lao Readiness

> Research date: 2026-08-21.

## 27 — Competitor lessons (deep, not wide)

### OrangeHRM (open source) — lessons
- **Leave/attendance/performance focus** — LaoHR matches + exceeds with Lao compliance.
- **No native payroll** — LaoHR has native Lao payroll (advantage).
- **Module-based architecture** — LaoHR's controller-per-domain is similar.
- **Lesson**: Keep modules independent; don't over-couple payroll to leave.

### ERPNext (open source) — lessons
- **Frappe framework workflows** — configurable approval workflows. LaoHR should adopt Stateless for this.
- **Native payroll** — table stakes. LaoHR has it.
- **MariaDB-backed** — LaoHR uses PostgreSQL (better for Lao search with ICU).
- **Lesson**: Configurable workflows are valuable but don't need a full Frappe-like framework.

### Odoo (open source) — lessons
- **Most feature-complete open-source HRIS** — payroll + expenses + recruitment + appraisals.
- **Modular marketplace** — LaoHR should stay modular but not build an app marketplace.
- **Lesson**: Feature completeness comes from incremental module addition, not a monolithic build.

### BambooHR (proprietary) — lessons
- **UX benchmark for self-service + onboarding** — clean, simple.
- **Cloud-only** — LaoHR needs on-prem option (Lao market reality).
- **Lesson**: UX simplicity matters; LaoHR's mock data and missing /403 page hurt UX trust.

### Zoho People (proprietary) — lessons
- **"AI-first" + Zia chatbot** — 2025/2026 trend. LaoHR should NOT pursue AI until Lao NLP quality is validated.
- **Separate products for payroll/expenses/recruit** — LaoHR bundles these (advantage for integrated experience).
- **Lesson**: AI is marketing-heavy; validate before building.

### What LaoHR should NOT copy
- Full ATS/recruitment module (P3 — no demand yet).
- 360 feedback / OKR / calibration (P3 — over-engineering).
- App marketplace / extension ecosystem.
- Cloud-only SaaS (Lao market needs on-prem).
- AI chatbot (Lao NLP quality unvalidated).

## 28 — Lao product priority matrix

| Feature | Lao regulatory importance | Lao operational importance | Global HR value | Complexity | Maintenance | Infrastructure | User frequency | Priority |
|---|---|---|---|---|---|---|---|---|
| **Update PIT brackets (2.5M threshold)** | CRITICAL | CRITICAL | — | LOW | LOW | NONE | Monthly (payroll) | **P0** |
| **Verify NSSF ceiling** | CRITICAL | HIGH | — | LOW | LOW | NONE | Monthly | **P0** |
| **Argon2id password hashing** | — | — | HIGH (security) | MEDIUM | LOW | NONE | Daily (login) | **P0** |
| **PG migrations** | — | HIGH | HIGH | MEDIUM | LOW | NONE | — | **P0** |
| **WAL+PITR backups** | — | CRITICAL | HIGH | LOW | LOW | LOW | — | **P0** |
| **PM/Finance/Knowledge tests** | — | HIGH | HIGH | MEDIUM | LOW | NONE | — | **P0** |
| **Fix employee routes** | — | HIGH | — | LOW | NONE | NONE | Daily | **P0** |
| **OvertimeEntry (day-type OT)** | HIGH | HIGH | MEDIUM | MEDIUM | LOW | NONE | Monthly | **P1** |
| **Taxable/non-taxable allowances** | HIGH | MEDIUM | MEDIUM | MEDIUM | LOW | NONE | Monthly | **P1** |
| **Personal/dependant deductions** | HIGH | MEDIUM | MEDIUM | MEDIUM | LOW | NONE | Monthly | **P1** |
| **Loan→payroll auto-deduction** | — | HIGH | MEDIUM | MEDIUM | LOW | NONE | Monthly | **P1** |
| **PIT monthly withholding report** | HIGH | HIGH | — | LOW | LOW | NONE | Monthly | **P1** |
| **ManagerId + dept hierarchy** | — | HIGH | HIGH | LOW | LOW | NONE | Daily | **P1** |
| **Approval engine (Stateless)** | — | MEDIUM | HIGH | MEDIUM | MEDIUM | NONE | Daily | **P1** |
| **Bulk employee import** | — | HIGH | MEDIUM | LOW | LOW | NONE | Occasional | **P1** |
| **Notifications (in-app + email)** | — | MEDIUM | HIGH | MEDIUM | MEDIUM | LOW | Daily | **P1** |
| **Timesheet** | — | MEDIUM | HIGH | MEDIUM | LOW | NONE | Daily | **P1** |
| **Minimum wage validation** | MEDIUM | MEDIUM | LOW | LOW | LOW | NONE | Monthly | **P1** |
| **Foreign employee fields** | MEDIUM | MEDIUM | MEDIUM | LOW | LOW | NONE | Occasional | **P2** |
| **Charts (SVG)** | — | MEDIUM | MEDIUM | MEDIUM | LOW | NONE | Daily | **P2** |
| **Mobile nav + PWA** | — | MEDIUM | HIGH | MEDIUM | MEDIUM | NONE | Daily | **P2** |
| **WCAG 2.2 AA** | — | LOW | HIGH | MEDIUM | LOW | NONE | — | **P2** |
| **NFC + ICU collation** | — | MEDIUM | MEDIUM | MEDIUM | LOW | NONE | Daily (search) | **P2** |
| **Annual holiday import** | MEDIUM | MEDIUM | LOW | LOW | LOW | NONE | Annual | **P2** |
| **Performance management** | — | MEDIUM | HIGH | HIGH | MEDIUM | NONE | Annual | **P2** |
| **Task dependencies + Gantt** | — | LOW | MEDIUM | MEDIUM | LOW | NONE | Daily (PM) | **P2** |
| **AI/RAG** | — | LOW | LOW | HIGH | HIGH | HIGH | — | **P3 (defer)** |
| **Recruitment/ATS** | — | LOW | MEDIUM | HIGH | MEDIUM | NONE | Occasional | **P3 (defer)** |
| **Multi-tenancy** | — | LOW | LOW | HIGH | HIGH | HIGH | — | **P3 (defer)** |

## 29 — AI Lao readiness

| Capability | Evidence | Lao quality | Confidence | Verdict |
|---|---|---|---|---|
| Lao OCR (Tesseract) | Tesseract has Lao traineddata | LOW for real-world scans | LOW | EXPERIMENTAL — pilot only |
| Lao OCR (Google Vision/Azure) | Commercial APIs support Lao | MEDIUM but cloud-dependent + cost | MEDIUM | EXPERIMENTAL — privacy concerns |
| Lao embeddings (multilingual-e5) | Includes Lao in training | UNKNOWN — low-resource language | UNKNOWN | Test before adopting |
| Lao LLMs (GPT-4/Claude) | Handle Lao to varying degrees | LOWER than English/Thai | LOW | Do NOT rely on for legal/compliance |
| Lao tokenization | No standard word segmenter | Lao has no spaces between words | MEDIUM | Research needed |
| Lao translation (Google) | Usable but not legal-grade | MEDIUM | MEDIUM | OK for UI/drafts, NOT legal docs |
| RAG (pgvector) | PostgreSQL extension, no new infra | Depends on embedding quality | UNKNOWN | P3 — defer until Lao embeddings validated |

### AI data privacy for HR
- HR AI involves salary, medical (sick leave), biometric, government ID data.
- Cloud AI (OpenAI/Google) sends PII to external servers — **privacy risk** without explicit consent + legal basis.
- Lao data protection law status: UNKNOWN (no confirmed PDPL).
- **Recommendation**: AI should remain **P3+ deferred** until: (a) Lao model quality validated, (b) privacy/compliance framework understood, (c) customer demand exists. If pursued, prefer on-prem/local models for PII-sensitive features. pgvector on existing PostgreSQL for RAG (no new infra).