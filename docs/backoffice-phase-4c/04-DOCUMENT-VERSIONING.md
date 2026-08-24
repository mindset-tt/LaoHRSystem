# 04 — DOCUMENT VERSIONING

## Invariant

Version numbers increase monotonically per document (1, 2, 3, …). A new version
appends a `DocumentVersion` row; the previous version is never overwritten.

## Implementation

`DocumentService.AddVersionAsync` reads the document, computes
`CurrentVersion + 1`, inserts a new `DocumentVersion`, and updates
`CurrentVersion`. `GetLatestVersionAsync` returns the highest version.

## Tests

- `AddVersion_IncrementsMonotonically` — v2 then v3, `CurrentVersion == 3`.
- `AddVersion_PreservesPreviousVersion` — prior version row retained.
- `GetLatestVersion_ReturnsHighest`.

## Status

PASS.
