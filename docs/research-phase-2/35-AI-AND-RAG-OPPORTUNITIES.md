# 35 — AI & RAG Opportunities + 36 — Lao NLP/OCR/Embeddings

> Research date: 2026-08-21. Confidence: MEDIUM (AI capabilities evolve rapidly; Lao-specific support is poorly documented).

## AI opportunities for LaoHR

| Feature | Value | Complexity | Lao relevance | Verdict |
|---|---|---|---|---|
| HR document OCR (extract from scans) | HIGH | HIGH | HIGH (Lao docs scanned) | EXPERIMENTAL — Lao OCR quality uncertain |
| Knowledge Q&A (policy assistant) | MEDIUM | MEDIUM | MEDIUM | EXPERIMENTAL — Lao LLM quality varies |
| Resume parsing | MEDIUM | MEDIUM | MEDIUM | EXPERIMENTAL — Lao resume parsing |
| Job description assistance | LOW | LOW | LOW | UNNECESSARY (no recruitment module) |
| HR analytics explanations | LOW | MEDIUM | LOW | UNNECESSARY |
| Report summaries | LOW | LOW | LOW | UNNECESSARY |
| Semantic search | MEDIUM | MEDIUM | MEDIUM (Lao) | EXPERIMENTAL |
| Document classification | LOW | MEDIUM | LOW | UNNECESSARY |
| Translation assistance (Lao↔English) | MEDIUM | LOW | HIGH | EXPERIMENTAL — useful for bilingual docs |

## Lao NLP / OCR / embeddings

| Capability | Evidence | Confidence | Verdict |
|---|---|---|---|
| Lao OCR | Tesseract has Lao traineddata; quality is LOW for real-world scans (poor print, mixed scripts). Commercial (Google Vision, Azure) better but cost + cloud dependency. | LOW | Do NOT rely on Lao OCR without testing. On-prem OCR is risky. |
| Lao embeddings | Multilingual models (e.g., multilingual-e5) include Lao but quality is UNKNOWN — Lao is low-resource. No dedicated Lao embedding model widely validated. | UNKNOWN | Test before adopting. |
| Lao LLMs | GPT-4/Claude handle Lao to varying degrees; quality is LOWER than English/Thai. Local LLMs (Llama) have minimal Lao training. | LOW | Do NOT assume "multilingual" = good Lao. Test with real HR text. |
| Lao tokenization | No standard Lao word segmenter (Lao has no spaces between words). This affects search, embeddings, NLP. | MEDIUM | Research Lao word segmentation libraries. |
| Lao translation | Google Translate Lao is usable but not legal-grade. | MEDIUM | Useful for UI/drafts, NOT for legal docs. |
| Lao speech | Not relevant for HRIS text system. | — | N/A |

## RAG research (78)

| Vector store | Complexity | LaoHR fit | Confidence |
|---|---|---|---|
| **PostgreSQL + pgvector** | 🟢 Low (extension on existing PG) | ✅ **Best fit** — no new infra | VERIFIED |
| Qdrant | 🟡 Medium (new service) | ⚠️ Only if pgvector insufficient | HIGH |
| Milvus | 🔴 High (new infra) | ❌ Overkill | HIGH |
| Weaviate | 🟡 Medium | ⚠️ | MEDIUM |
| Elasticsearch | 🔴 High | ❌ Overkill for search + vector | HIGH |
| OpenSearch | 🔴 High | ❌ | MEDIUM |

### Verdict: PostgreSQL + pgvector (if RAG is pursued)

- LaoHR already runs PostgreSQL 16. `pgvector` is an extension — no new infrastructure.
- For an HR knowledge assistant: chunk knowledge articles → embed → store in pgvector → cosine similarity search.
- **BUT**: Lao embedding quality is UNKNOWN. Test multilingual-e5 or similar with real Lao HR text before committing.
- **Recommendation**: RAG is **P3 / EXPERIMENTAL**. Do NOT build until: (a) Lao embedding quality is validated, (b) customer demand exists. If pursued, use pgvector on existing PostgreSQL. **Keep lightweight.**

## Final AI verdict

| Category | Recommendation |
|---|---|
| HIGH VALUE | None currently — Lao AI quality too uncertain for production HR |
| EXPERIMENTAL | Lao OCR, knowledge Q&A, translation assistance — pilot only |
| UNNECESSARY | Job description AI, analytics explanations, report summaries, doc classification |
| RISKY | Relying on Lao LLM output for legal/compliance decisions without human review |

**Do NOT add AI features in Phase 3.** AI is P3+ and requires validation with real Lao text + human-in-the-loop.