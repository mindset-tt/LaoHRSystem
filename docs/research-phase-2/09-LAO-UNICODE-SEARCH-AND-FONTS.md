# 09 — Lao Unicode, Search & Fonts (ຊອກຫາ / ຕົວອັກສອນ)

> Research date: 2026-08-21. Confidence: VERIFIED (Unicode, pg_trgm) / MEDIUM (Lao-specific search).

## Lao Unicode

| Item | Value | Confidence |
|---|---|---|
| Unicode block | `U+0E80`–`U+0EFF` (Lao block) | VERIFIED |
| Encoding | UTF-8 | VERIFIED |
| Script type | Abugida (consonant + vowel diacritics + tone marks) | VERIFIED |
| Combining marks | Yes — vowels and tone marks combine above/below/around consonants | VERIFIED |
| Normalization | NFC recommended (Lao uses combining marks; NFD would decompose) | HIGH |
| Direction | Left-to-right (LTR) | VERIFIED |
| Case | No uppercase/lowercase (case-insensitive search is N/A for Lao script) | VERIFIED |

### Unicode challenges for Lao

1. **Combining marks**: Lao vowels ( ີ ື ຸ ເ ແ ໂ ໃ ໄ) and tone marks (່ ້ ໊ ໋) combine with consonants. A "character" in Lao is a syllable cluster, not a single code point.
2. **Normalization**: Input may arrive in different normalization forms (NFC vs NFD). PostgreSQL + .NET should normalize to NFC for consistent storage and comparison.
3. **Sorting**: Lao has a specific collation order (consonants → vowels → tone marks). PostgreSQL needs ICU collation for correct Lao sorting.
4. **Case-insensitive search**: Irrelevant for Lao script (no case), but Lao + Latin mixed names need both `ILIKE` (for Latin) and normalized comparison (for Lao).

## PostgreSQL collation & search

| Feature | PostgreSQL support | Confidence | Priority |
|---|---|---|---|
| ICU collation for Lao | `CREATE COLLATION lo_LA (provider = icu, locale = 'lo_LA')` — ICU supports Lao | HIGH | P2 |
| `pg_trgm` for fuzzy search | Works but may produce poor results for Lao (trigrams split syllable clusters) | VERIFIED (pg_trgm) / MEDIUM (Lao quality) | P2 |
| Full-text search (`tsvector`) | `simple` dictionary works for Lao (no Lao stemmer); word boundaries are syllable-based | MEDIUM | P3 |
| `ILIKE` | Works for Latin; for Lao, normalization matters | VERIFIED | — |
| `citext` | Case-insensitive text — useful for Latin, N/A for Lao | VERIFIED | P3 |

### Recommendation for Lao search

1. **P2**: Normalize all Lao text to **NFC** on input (`.NET`: `text.Normalize(NormalizationForm.FormC)`).
2. **P2**: Create an **ICU collation** for Lao (`lo_LA`) for correct sorting.
3. **P2**: For name search, use `ILIKE` with NFC-normalized input + `pg_trgm` GIN index for fuzzy matching. Test with real Lao names — adjust `similarity_threshold` if needed.
4. **P3**: If global search is needed, evaluate `tsvector` with `simple` dictionary (no Lao stemmer exists). Do NOT add Elasticsearch unless pg_trgm + tsvector proves insufficient at scale.

**Verdict**: PostgreSQL is sufficient for Lao search at LaoHR's scale (50–5,000 employees). No need for Elasticsearch/MeiliSearch. **Keep lightweight.**

## Lao fonts

| Font | Type | License | Web-compatible | Confidence | Use case |
|---|---|---|---|---|---|
| **Noto Sans Lao** | Sans-serif | SIL Open Font License (OFL) | ✅ Google Fonts | VERIFIED | UI body text |
| **Noto Serif Lao** | Serif | OFL | ✅ Google Fonts | VERIFIED | Headings, formal docs |
| **Phetsarath OT** | Sans-serif | OFL (Lao Ministry of Post & Telecom) | ✅ | VERIFIED | Government docs (LaoHR already uses for PDF payslips) |
| **Saysettha OT** | Sans-serif | OFL | ✅ | HIGH | Alternative |

**LaoHR current**: Inter (UI) + Noto Sans Lao (Lao text) via Google Fonts `@import` in `globals.css`. Phetsarath OT registered for QuestPDF payslips. **VERIFIED good choices.**

**GAP**: None for current scope. If formal government documents need serif, add Noto Serif Lao. **P3.**

## Lao text input

| Platform | Support | Confidence |
|---|---|---|
| Windows | Built-in Lao keyboard (Lao PDR standard layout) | VERIFIED |
| macOS | Built-in Lao keyboard | VERIFIED |
| Linux | IBUS + Lao keyboard | HIGH |
| Android | Built-in Lao keyboard (Gboard) | VERIFIED |
| iOS | Built-in Lao keyboard | VERIFIED |
| IME behavior | Direct input (no complex IME conversion like Japanese) — Lao is typed directly | VERIFIED |
| Combining character edge cases | Vowels/tone marks may be typed in any order → non-canonical sequences. NFC normalization on input prevents this. | HIGH |

**Testing requirements**:
- Test input with Lao keyboard on Windows + macOS + mobile.
- Test copy/paste from Word/PDF (may carry formatting + non-NFC normalization).
- Test search with partial Lao names (missing tone marks).
- Test sorting with mixed Lao/Latin names.

**GAP**: No automated Lao input testing. **P2** — add Lao test data + input tests.