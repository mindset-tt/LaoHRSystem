# 37 — Release Artifacts

## Artifacts
- Backend: Docker image (multi-stage, non-root).
- Frontend: Docker image (Next.js standalone).
- Migrations: EF Core migrations (applied at startup or via `dotnet ef`).

## Findings
- Dockerfiles exist for API and Web.
- No versioned image tags / release tagging strategy.

## Follow-up
- Tag images with semantic version + git SHA.
- Sign images (cosign) — recommended.
- Publish SBOM (Software Bill of Materials).
- Document release versioning (see 38-RELEASE-REHEARSAL).
