# 21 — Acceptance Criteria

## Docker

- [x] Fresh clone + `.env` + `docker compose up --build` starts all services.
- [x] No local .NET, Node, or PostgreSQL installation is needed.
- [x] API is reachable at `http://localhost:8080`.
- [x] Frontend is reachable at `http://localhost:3000`.
- [x] Database survives `docker compose down` then `docker compose up`.

## API

- [x] `POST /classes` creates an admission class.
- [x] `GET /classes` returns seat counts.
- [x] `GET /classes/{classId}` returns one class and counts.
- [x] `POST /classes/{classId}/holds` requires `X-Student-Id`.
- [x] No capacity returns `409`.
- [x] `GET /holds/{holdId}` returns the hold status.
- [x] `POST /holds/{holdId}/confirm` confirms valid hold.
- [x] Repeated confirm returns success without double counting.
- [x] `DELETE /holds/{holdId}` cancels eligible hold with `204`.
- [x] `GET /me/holds` lists the current student's holds.
- [x] Ownership is enforced.

## Business correctness

- [x] Active holds plus confirmed admissions never exceed total seats.
- [x] Hold duration comes from `HOLD_DURATION_SECONDS`, default 600.
- [x] Expired seats become available without manual intervention.
- [x] One student cannot keep multiple active holds.
- [x] Invalid inputs return 4xx, not avoidable 500.
- [x] Time calculations use UTC.

## UI

- [x] Parent can enter student ID and view classes.
- [x] Parent can apply for an available seat.
- [x] Parent can view all own applications.
- [x] Parent can simulate payment and confirm.
- [x] Parent can cancel an active hold.
- [x] Expired application displays EXPIRED and disables payment.
- [x] Refresh/reopen restores the student's applications.
- [x] Admin can create a class and view counts.
- [x] Errors are understandable; no blank page or console-only errors.

## Architecture and quality

- [x] Domain has no infrastructure/framework dependency.
- [x] Application contains use cases and ports.
- [x] Infrastructure implements persistence.
- [x] API contains HTTP concerns only.
- [x] Frontend never connects directly to PostgreSQL.
- [x] Unit, integration, frontend, and concurrency tests run in CI.
- [x] `.env` and secrets are not committed.
