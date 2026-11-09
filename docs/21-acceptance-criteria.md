# 21 — Acceptance Criteria

## Docker

- [ ] Fresh clone + `.env` + `docker compose up --build` starts all services.
- [ ] No local .NET, Node, or PostgreSQL installation is needed.
- [ ] API is reachable at `http://localhost:8080`.
- [ ] Frontend is reachable at `http://localhost:3000`.
- [ ] Database survives `docker compose down` then `docker compose up`.

## API

- [ ] `POST /classes` creates an admission class.
- [ ] `GET /classes` returns seat counts.
- [ ] `GET /classes/{classId}` returns one class and counts.
- [ ] `POST /classes/{classId}/holds` requires `X-Student-Id`.
- [ ] No capacity returns `409`.
- [ ] `GET /holds/{holdId}` returns the hold status.
- [ ] `POST /holds/{holdId}/confirm` confirms valid hold.
- [ ] Repeated confirm returns success without double counting.
- [ ] `DELETE /holds/{holdId}` cancels eligible hold with `204`.
- [ ] `GET /me/holds` lists the current student's holds.
- [ ] Ownership is enforced.

## Business correctness

- [ ] Active holds plus confirmed admissions never exceed total seats.
- [ ] Hold duration comes from `HOLD_DURATION_SECONDS`, default 600.
- [ ] Expired seats become available without manual intervention.
- [ ] One student cannot keep multiple active holds.
- [ ] Invalid inputs return 4xx, not avoidable 500.
- [ ] Time calculations use UTC.

## UI

- [ ] Parent can enter student ID and view classes.
- [ ] Parent can apply for an available seat.
- [ ] Parent can view all own applications.
- [ ] Parent can simulate payment and confirm.
- [ ] Parent can cancel an active hold.
- [ ] Expired application displays EXPIRED and disables payment.
- [ ] Refresh/reopen restores the student's applications.
- [ ] Admin can create a class and view counts.
- [ ] Errors are understandable; no blank page or console-only errors.

## Architecture and quality

- [ ] Domain has no infrastructure/framework dependency.
- [ ] Application contains use cases and ports.
- [ ] Infrastructure implements persistence.
- [ ] API contains HTTP concerns only.
- [ ] Frontend never connects directly to PostgreSQL.
- [ ] Unit, integration, frontend, and concurrency tests run in CI.
- [ ] `.env` and secrets are not committed.
