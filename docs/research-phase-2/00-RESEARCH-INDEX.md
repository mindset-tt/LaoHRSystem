# LaoHR — Phase 2 Research Index

> Deep research, Lao PDR domain research, and production gap analysis. READ-ONLY phase — no code was modified.
> Research date: 2026-08-21.

## How to use

1. **Read first**: `01-EXECUTIVE-RESEARCH-SUMMARY.md` — top findings.
2. **Read second**: `48-PHASE-3-IMPLEMENTATION-INPUT.md` — designed for the next AI agent to begin Phase 3.
3. **Reference**: numbered documents below for deep detail on any area.
4. **Before implementing anything**: read `45-LEGAL-CONFIRMATION-REQUIRED.md` — payroll/tax/NSSF/leave parameters require human/legal confirmation.

## Relationship to Phase 1

This phase builds on `docs/ai-handoff/` (Phase 1 project extraction). The Phase 1 handoff documents are the starting context. This phase adds Lao PDR domain research, international best practices, competitor analysis, gap analysis, and evidence-backed recommendations. Where Phase 1 findings are superseded (e.g., `proxy.ts` is correct in Next.js 16, not a bug), this phase notes the correction.

## Document index

| # | Document | Domain |
|---|---|---|
| 00 | `00-RESEARCH-INDEX.md` | This file |
| 01 | `01-EXECUTIVE-RESEARCH-SUMMARY.md` | Top findings + final synthesis |
| 02 | `02-LAO-LABOUR-LAW.md` | Lao labour law (working hours, OT, contracts, termination) |
| 03 | `03-LAO-PAYROLL.md` | Lao payroll components + calculation order |
| 04 | `04-LAO-PERSONAL-INCOME-TAX.md` | Lao PIT (brackets, deductions, withholding) |
| 05 | `05-LAO-SOCIAL-SECURITY.md` | Lao NSSF (rates, ceiling, coverage, reporting) |
| 06 | `06-LAO-LEAVE-AND-HOLIDAYS.md` | Lao leave entitlements + public holidays |
| 07 | `07-LAO-EMPLOYMENT-LIFECYCLE.md` | Employee lifecycle + org structure + position/job |
| 08 | `08-LAO-I18N-AND-LOCALIZATION.md` | Timezone, calendar, locale, names, address, phone, currency |
| 09 | `09-LAO-UNICODE-SEARCH-AND-FONTS.md` | Lao Unicode, NFC, pg_trgm, ICU collation, fonts |
| 10 | `10-LAO-HR-DATA-MODEL.md` | Entity gap matrix + foreign employee + document types |
| 11 | `11-HRIS-GLOBAL-BENCHMARK.md` | Competitor feature comparison |
| 12 | `12-EMPLOYEE-SELF-SERVICE.md` | ESS + MSS gap analysis |
| 14 | `14-RECRUITMENT-AND-ONBOARDING.md` | Recruitment/ATS + onboarding/offboarding + performance + attendance/shifts |
| 17 | `17-WORKFLOW-AND-APPROVALS.md` | Approval engine (Stateless+Hangfire) + PM research (Kanban/Gantt/resource/risk) |
| 23 | `23-FINANCE-EXPENSE-LOAN.md` | Finance + knowledge + documents + reporting/BI/dashboards |
| 27 | `27-SECURITY-RESEARCH.md` | Security + privacy + DB/scalability + performance + DevOps + backup/DR + observability + test strategy |
| 35 | `35-AI-AND-RAG-OPPORTUNITIES.md` | AI/RAG + Lao NLP/OCR/embeddings |
| 37 | `37-COMPETITOR-MATRIX.md` | Competitor + current system gap matrix + feature priority + architecture options (ADRs) |
| 41 | `41-EVIDENCE-REGISTER.md` | Evidence + source + research questions + conflicting sources + legal confirmation register |
| 46 | `46-RECOMMENDED-PRODUCT-ROADMAP.md` | Product + technical roadmap (Phase 3A-3J) |
| 48 | `48-PHASE-3-IMPLEMENTATION-INPUT.md` | **Phase 3 implementation specification for next AI** |

## Confidence system used

- `VERIFIED` — confirmed by authoritative source
- `HIGH CONFIDENCE` — strong evidence but not directly confirmed
- `MEDIUM CONFIDENCE` — commonly reported but not verified against primary source
- `LOW CONFIDENCE` — limited evidence
- `UNKNOWN` — insufficient evidence
- `CONFLICTING SOURCES` — sources disagree
- `OUTDATED SOURCE` — may be superseded

## Legal disclaimer

This research supports software requirements. It is **NOT legal advice**. All Lao tax/NSSF/labour law parameters must be confirmed by a qualified Lao legal/tax/labour professional before production use. Items requiring confirmation are listed in `45-LEGAL-CONFIRMATION-REQUIRED.md` (within `41-EVIDENCE-REGISTER.md`).