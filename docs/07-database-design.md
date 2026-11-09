# 07 — Database Design

Use PostgreSQL and EF Core migrations. Use UTC timestamps (`timestamp with time zone` / `timestamptz`).

## `admission_classes`

| Column | Type | Rules |
|---|---|---|
| id | uuid | PK |
| name | varchar(160) | NOT NULL |
| total_seats | integer | NOT NULL, CHECK > 0 |
| created_at_utc | timestamptz | NOT NULL |

## `seat_holds`

| Column | Type | Rules |
|---|---|---|
| id | uuid | PK |
| admission_class_id | uuid | FK admission_classes |
| student_id | varchar(64) | NOT NULL |
| status | varchar(16) | NOT NULL |
| created_at_utc | timestamptz | NOT NULL |
| expires_at_utc | timestamptz | NOT NULL |
| confirmed_at_utc | timestamptz | NULL |
| cancelled_at_utc | timestamptz | NULL |

## Indexes

- `seat_holds(admission_class_id, status, expires_at_utc)`
- `seat_holds(student_id, status, expires_at_utc)`
- Optional partial unique index for one active hold per student:
  `UNIQUE(student_id) WHERE status = 'HELD'`.
  Because expired rows may still be marked `HELD` until processed, this index alone is insufficient for time-based uniqueness. Expire stale holds transactionally before inserting a new hold. If using the partial unique index, ensure the expiry transition is performed under the same concurrency protocol.

## Seat counts

- `heldSeats`: count rows with `status = 'HELD' AND expires_at_utc > now`.
- `confirmedSeats`: count rows with `status = 'CONFIRMED'`.
- `availableSeats`: `totalSeats - heldSeats - confirmedSeats`.

Expired holds must not count as held even if a background process has not yet changed their stored status. On access/hold creation, update expired records to `EXPIRED` as part of a transaction.

## Persistence

Configure a named Docker volume for PostgreSQL data. Migrations must run automatically or through a documented one-shot migration service before API readiness. Never use `EnsureDeleted` in application startup.
