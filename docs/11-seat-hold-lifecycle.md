# 11 — Seat Hold Lifecycle

## Create

A hold is created only when:
- Admission class exists.
- Student ID is valid.
- Student has no other active hold (per assumptions).
- A seat is available after expired holds are processed.

Expiry = current UTC time + configured duration.

## Read

Before returning hold status, evaluate expiry. If status is `HELD` and expiry is at or before now, persist `EXPIRED`.

## Confirm

Within a transaction:
1. Verify student ownership.
2. If already `CONFIRMED`, return success unchanged.
3. If not `HELD`, reject with conflict.
4. If expiry has passed, transition to `EXPIRED` and reject with conflict.
5. Transition to `CONFIRMED`, set `confirmed_at_utc`, commit.

No real money is collected.

## Cancel

Within a transaction:
1. Verify ownership.
2. If `HELD` and not expired, transition to `CANCELLED`.
3. If already `CANCELLED`, return idempotent success.
4. Reject other terminal states according to the documented policy.

## Expiry without background jobs

A background worker is optional. Correctness must not depend on one. Expired records must be excluded from counts and transitioned during relevant reads/commands. A hosted background service may periodically sweep expired rows for tidy status reporting, but the API must remain correct if the worker is stopped.
