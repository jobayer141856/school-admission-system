# 02 — Assumptions

The assignment intentionally leaves some details open. These assumptions make the implementation deterministic.

| Topic | Assumption | Reason |
|---|---|---|
| Admission unit | One record represents one class + one shift, e.g. `Class 6 - Morning Shift`. | Capacity is explicitly separate for each class and shift. |
| Student identity | `X-Student-Id` contains the student's birth-registration number as entered by the parent. | The brief requires this header and says the birth-registration number is the student's identity. |
| Student ID validation | Trim whitespace; require a non-empty value; validate allowed length/characters using a configurable, documented rule. Do not hard-code the example value as a real identity. | Prevent blank or malformed ownership keys. |
| Hold duration | `HOLD_DURATION_SECONDS`, default `600`. | Explicit assignment requirement. |
| Hold policy | A student can have at most one unexpired `HELD` hold at a time across the system. | The brief says a student cannot keep multiple active holds. This conservative interpretation avoids reserving multiple seats. |
| Duplicate hold request | If the same student repeats a request for the same admission unit while their hold is active, return the existing active hold rather than allocating another seat. If they have an active hold for another unit, return `409`. | Makes retries safe and prevents duplicate active reservations. |
| Expiry | A hold is expired when `expires_at <= current UTC time`. Expiry is evaluated on reads and before capacity allocation/confirmation. | Prevents stale holds from occupying seats. |
| Payment | Payment is simulated and succeeds when the user clicks “Pay admission fee”, provided the hold belongs to that student and has not expired/cancelled. | The brief explicitly says no real payment gateway is needed. |
| Confirmation idempotency | Repeated confirmation of an already confirmed hold returns `200` with `CONFIRMED`, without incrementing confirmed capacity again. | Explicit assignment rule. |
| Cancellation | `DELETE` cancels a `HELD` hold. A confirmed hold cannot be cancelled through this endpoint and returns `409`. | The brief says cancellation exists but does not define post-confirmation refund policy. |
| Admin authentication | No admin login is specified. The demo admin screen is unauthenticated and must be labelled as a take-home/demo assumption. Do not present this as production-secure. | Avoid inventing an auth contract that conflicts with the supplied API. |
| Admin class editing | Only class creation is required; no update/delete operation is added to the required API. | Stay within the supplied contract. |
| Seat counts | `heldSeats` counts only unexpired `HELD` records; `confirmedSeats` counts `CONFIRMED`; `availableSeats = totalSeats - heldSeats - confirmedSeats`. | Matches the contract's count semantics. |
| Time | Store timestamps in UTC; serialize as ISO-8601 UTC. | Consistent expiry handling across machines. |
| IDs | Use UUID internally; map to readable prefixed IDs such as `c_<uuid>` and `h_<uuid>` at API boundaries. | The brief provides prefixed example IDs but does not prescribe a format. |
| API versioning | Keep the supplied paths unversioned for compatibility. | The evaluator may call the exact routes. |
| Frontend routes | `/` is the parent view; `/admin` is the admin view. | Simple, discoverable UI split. |

Any change to these assumptions must be documented here and reflected in tests and API docs.
