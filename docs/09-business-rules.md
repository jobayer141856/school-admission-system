# 09 — Business Rules (Mandatory)

1. For any admission class, `held + confirmed` must never exceed `totalSeats`, regardless of simultaneous requests.
2. `HOLD_DURATION_SECONDS` configures hold duration; default is `600`.
3. Expired seats return to availability automatically through API processing; no manual API call is required.
4. A student may not keep more than one active `HELD` application at a time. Apply the exact policy documented in `02-assumptions.md`.
5. Only the student who owns a hold may inspect, confirm, or cancel it.
6. Repeating confirmation must not increase `confirmedSeats` more than once.
7. Application restart must not lose data, including after `docker compose down` then `docker compose up`.
8. Invalid input must return 4xx, never an avoidable 500.
9. All parent/admin actions must be available through the UI; no Postman or direct database action should be required.
10. After expiry, the UI must show `EXPIRED` and disable the payment/confirm button.
11. Refreshing the page or reopening the browser must restore the student's holds.
12. Errors such as no seats, expired hold, invalid input, and unavailable server must be understandable to users.

## Status semantics

- `HELD`: temporary reservation with `expiresAt > now`.
- `CONFIRMED`: simulated payment succeeded.
- `EXPIRED`: hold elapsed before confirmation.
- `CANCELLED`: parent cancelled an eligible hold.

## Clock rules

Use an injected clock (`TimeProvider` or `IClock`) in application/domain tests. Never use local machine time in domain rules. Store and compare UTC timestamps.

## Failure behavior

If a transaction fails, roll back the entire operation. Never leave a seat count incremented without the corresponding hold/confirmation record.
