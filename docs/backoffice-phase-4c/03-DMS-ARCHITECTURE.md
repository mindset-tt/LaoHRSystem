# 03 — DMS ARCHITECTURE

Corporate Document Management System.

## Design

- `CorporateDocument` is polymorphic: `OwnerEntityType` + `OwnerEntityId` link it
  to any domain (EMPLOYEE, SUPPLIER, PURCHASE_ORDER, CONTRACT, ASSET, VEHICLE,
  TRAVEL_REQUEST, SERVICE_REQUEST, PROJECT, GENERAL).
- `DocumentVersion` holds the actual file metadata; `CorporateDocument.CurrentVersion`
  points to the latest version.
- Storage is single-host friendly: `StorageReference` is a path/identifier; no S3,
  MinIO, or Azure Blob.

## Classification

- `DocumentType`: POLICY, CONTRACT, INVOICE, QUOTATION, RECEIPT, CERTIFICATE,
  LICENSE, IDENTIFICATION, ASSET_DOCUMENT, VEHICLE_DOCUMENT, TRAVEL_DOCUMENT,
  PROJECT_DOCUMENT, GENERAL.
- `Confidentiality`: INTERNAL, CONFIDENTIAL, RESTRICTED.

## Authorization

- `CanViewDocuments` / `CanManageDocuments` (Admin/HR/Finance).
- Document access is independent of parent-entity visibility: a user who can see
  a PO does not automatically see a RESTRICTED document attached to it.

## API

`/api/corporate-documents` — list (filtered by owner/type), get, create, list
versions, add version.

## Tests

`DocumentServiceTests` — monotonic versioning, previous-version preservation,
latest-version retrieval.
