# 06 — Domain Model

## AdmissionClass

Properties:
- `Id: AdmissionClassId`
- `Name: string`
- `TotalSeats: int`
- `CreatedAtUtc: DateTimeOffset`

Behavior:
- `Create(name, totalSeats, now)`
- Validate name and positive capacity.

## SeatHold

Properties:
- `Id: SeatHoldId`
- `AdmissionClassId: AdmissionClassId`
- `StudentId: StudentId`
- `Status: SeatHoldStatus`
- `CreatedAtUtc: DateTimeOffset`
- `ExpiresAtUtc: DateTimeOffset`
- `ConfirmedAtUtc: DateTimeOffset?`
- `CancelledAtUtc: DateTimeOffset?`

Status enum:
- `HELD`
- `CONFIRMED`
- `EXPIRED`
- `CANCELLED`

Behavior:
- `Create(...)`
- `Confirm(now)`
- `Expire(now)`
- `Cancel(now)`
- `IsExpired(now)`

## Value objects

- `StudentId`: normalized, validated student birth-registration identifier.
- `AdmissionClassId`: internal UUID wrapper if useful.
- `SeatHoldId`: internal UUID wrapper if useful.

Keep value objects lightweight; avoid unnecessary ceremony.

## State transitions

```text
             +----------+
             |   HELD   |
             +----------+
              /   |    \
             /    |     \
      confirm   expire  cancel
           /      |       \
          v       v        v
   +-----------+ +---------+ +-----------+
   | CONFIRMED | | EXPIRED | | CANCELLED |
   +-----------+ +---------+ +-----------+
```

Terminal states do not transition to another state. Repeating confirmation on `CONFIRMED` is an idempotent no-op at the use-case level.
