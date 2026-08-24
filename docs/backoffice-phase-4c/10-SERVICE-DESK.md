# 10 — SERVICE DESK

## Design

Extends the existing `ServiceRequest` + `ServiceRequestCategory`. Added
`ServiceRequestHistory` (append-only) for assignment/status/priority changes.

## Flow

Employee submits → Admin/HR assigns → assignee progresses → comments (reused
`EntityComment`, now allow-listed for SERVICE_REQUEST) → resolves → closes.

## API

`ServiceRequestsController` extended with `history`; `UpdateRequest` now records
history on status/assignment/priority changes.

## Status

PASS.
