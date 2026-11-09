# 04 — DDD Strategy

## Bounded context

The application has one bounded context: **School Admission**.

## Ubiquitous language

- **Admission Class**: a specific class-and-shift unit with a finite seat capacity.
- **Seat**: one capacity slot in an Admission Class; seats are not individual persisted rows.
- **Hold**: temporary reservation of one seat for a student.
- **Held**: hold is active and not expired.
- **Confirmed**: simulated payment succeeded and admission is confirmed.
- **Expired**: hold passed its expiry time without confirmation.
- **Cancelled**: parent cancelled an eligible active hold.
- **Available seats**: capacity minus active holds minus confirmed admissions.

## Aggregates

### AdmissionClass aggregate

Root: `AdmissionClass`.

Owns:
- ID
- Name
- TotalSeats
- CreatedAtUtc

Invariants:
- Name is required and bounded in length.
- TotalSeats must be positive.
- TotalSeats cannot be reduced below already held + confirmed seats (if editing is added later).
- A class cannot be created with invalid capacity.

### SeatHold aggregate

Root: `SeatHold`.

Owns:
- HoldId
- AdmissionClassId
- StudentId
- Status
- CreatedAtUtc
- ExpiresAtUtc
- ConfirmedAtUtc (nullable)
- CancelledAtUtc (nullable)

Invariants:
- New hold starts as `HELD`.
- Only `HELD` may transition to `CONFIRMED`, `EXPIRED`, or `CANCELLED`.
- Confirmation after expiry is forbidden.
- Confirming an already confirmed hold is idempotent at the application/API boundary.
- Ownership must be checked before exposing or changing a hold.

## Domain services

Use a domain service only where a rule does not naturally belong to one aggregate. Capacity coordination is a transactional application/domain operation because it spans AdmissionClass and SeatHold records.

## Domain events (optional)

Do not add an event bus for this small assignment. Domain events such as `SeatHoldConfirmed` may be introduced if they simplify decoupling, but avoid infrastructure complexity without a use case.

## Persistence model

Use relational tables `admission_classes` and `seat_holds`. Store student ID as a normalized string. Add indexes for class/status/expiry and student/status.
