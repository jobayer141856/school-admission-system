# 16 — Testing Strategy

## Domain tests

- AdmissionClass rejects empty name and non-positive capacity.
- SeatHold starts `HELD`.
- Valid hold can be confirmed.
- Expired hold cannot be confirmed.
- Cancelled hold cannot be confirmed.
- Repeated confirmation does not change state or timestamp unexpectedly.

## Application tests

- Create class validation.
- Create hold with available capacity.
- Reject hold when class is full.
- Reject second active hold for a student according to assumptions.
- Expired holds do not consume capacity.
- Student ownership is enforced.
- Confirm is idempotent.
- Cancel frees capacity.

Use a fake clock (`TimeProvider`) for deterministic expiry tests.

## Infrastructure/integration tests

Use PostgreSQL Testcontainers, not EF Core InMemory, for:
- Migrations apply successfully.
- Repositories persist and retrieve records.
- Row-lock capacity behavior.
- Concurrent hold creation.
- Restart/persistence behavior where practical.

## API integration tests

Verify exact routes, headers, status codes, JSON property names, and response shapes from `08-api-contract.md`.

## Required concurrency test

For a class with capacity `N`, run more than `N` simultaneous requests using distinct student IDs. Assert:
- Successful holds never exceed `N`.
- `heldSeats + confirmedSeats <= totalSeats`.
- Remaining capacity is never negative.
- Same-student simultaneous requests do not create multiple active holds.

## Frontend tests

- Student ID validation and persistence.
- Class list renders counts.
- Apply sends `X-Student-Id`.
- 409 capacity response shows readable feedback.
- Active hold displays countdown/status.
- Expired hold disables payment.
- Confirm updates status and seat counts.
- Admin can create a class.
- Network failure shows an error state.

## Suggested commands

```bash
# Backend
dotnet test backend/SchoolAdmission.sln

# Frontend (from frontend/)
npm ci
npm run test
npm run build
```

Adapt solution paths to the final generated solution file. CI must run backend tests, frontend tests, and frontend build.
