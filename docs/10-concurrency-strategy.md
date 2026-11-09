# 10 — Concurrency Strategy

## Problem

Two or more requests can attempt to reserve the final seat simultaneously. A simple `COUNT(*)` followed by `INSERT` is unsafe because both requests may observe the same remaining capacity.

## Required guarantee

For every admission class at every committed transaction boundary:

`activeHeldCount + confirmedCount <= totalSeats`

## Recommended PostgreSQL approach

Use a database transaction and serialize capacity-changing operations per admission class by locking the corresponding `admission_classes` row.

1. Begin transaction.
2. Select the target admission class row `FOR UPDATE`.
3. Mark expired holds for that class as `EXPIRED`.
4. Check whether the student already has an active hold.
5. Count active `HELD` and `CONFIRMED` holds.
6. If count is at capacity, roll back and return `409`.
7. Insert the new `HELD` record with expiry `now + HOLD_DURATION_SECONDS`.
8. Commit.
9. Return the created hold.

Use parameterized SQL when issuing `SELECT ... FOR UPDATE`; never interpolate user input.

## Confirm/cancel locking

- Lock the associated admission-class row first, then lock/read the hold.
- Apply expiry/status transition and update the hold in one transaction.
- Use the same lock ordering in all capacity-changing use cases to avoid deadlocks.
- Confirmation changes a hold from `HELD` to `CONFIRMED`; the total occupied count remains constant.
- Cancellation/expiry frees capacity.
- Repeated confirmation must detect `CONFIRMED` and return success without a second state change.

## Student-level uniqueness

To enforce one active hold per student across different classes, serialize operations by a stable student lock (e.g. PostgreSQL transaction-scoped advisory lock based on a stable hash of normalized student ID) before checking for an active hold. Then lock the class row. All code paths must use the same lock order: student lock, then class row.

Alternatively, document a simpler policy if the assignment interpretation changes. Do not rely only on frontend button disabling.

## Isolation

`READ COMMITTED` plus explicit row locks is sufficient when all capacity mutations follow the locking protocol. Do not claim correctness unless every mutation path uses the same protocol.

## Required automated concurrency test

- Seed a class with capacity `N`.
- Send substantially more than `N` simultaneous hold requests using distinct student IDs.
- Assert exactly `N` requests succeed (or fewer only if a separately documented global rule applies).
- Assert the class response reports `heldSeats + confirmedSeats <= totalSeats`.
- Repeat the test enough times to expose race conditions.
- Also test many simultaneous requests for the same student; no more than one active hold may exist.

Use PostgreSQL/Testcontainers for this test. EF Core InMemory does not validate PostgreSQL locking behavior.
